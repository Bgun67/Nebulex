using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct AStarCost{
    public float F;
    public float G;
    public float H;
}

[System.Serializable]
public class NavBlock{
    public Bounds m_bounds;
    public List<int> links = new List<int>();
    public float cost = -1f;
    public AStarCost costs;
    public Vector3 closestPoint;
}



[ExecuteInEditMode]
public class Nav_Volume_Builder : MonoBehaviour
{
    public bool visualize = false;
    bool visualizeChecked = false;
    public bool buildCubemesh;
    public Bounds bounds;
    public int numCubes;
    const int size = 60;
    public float scale = 5f;
    
    public bool[,,] mesh = new bool[size,size,size];
    public List<NavBlock> navVolume = new List<NavBlock>();

    public int currentShortestLength = 1000000;
    public float currentClosestPosition;
    public List<Vector3> plannedRoute = new List<Vector3>();
   
    public Transform endTransform;
    public bool findRoute = false;
    public Material cubeMaterial;

    public List<int> openList;
    public List<int> closedList;

    public int currentBlockIndex = 0;

    public static Nav_Volume_Builder _instance;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Nav_Volume_Builder._instance = this;

    }

    public void FindRoute(Vector3 start, Vector3 end){
       
        plannedRoute.Clear();
        closedList.Clear();
        openList.Clear();

        currentShortestLength = 1000000;
        currentClosestPosition = 1000000f;
        NavBlock endBlock = new NavBlock();
        float shortestDistance = float.MaxValue-1f;
        Vector3 closestPoint = Vector3.zero;
        foreach(NavBlock block in navVolume){
            block.cost = -1;
            block.costs.F = 1000000;
            block.costs.G = 1000000;
            Vector3 absoluteDisplacement = end-block.m_bounds.center;
            absoluteDisplacement = Vector3.Max(absoluteDisplacement, -absoluteDisplacement);
            absoluteDisplacement = new Vector3(
                Mathf.Clamp(absoluteDisplacement.x - block.m_bounds.extents.x/2f, 0, 10000f),
                Mathf.Clamp(absoluteDisplacement.y - block.m_bounds.extents.y/2f, 0, 10000f),
                Mathf.Clamp(absoluteDisplacement.z - block.m_bounds.extents.z/2f, 0, 10000f));
            Vector3 pointToTest = block.m_bounds.ClosestPoint(end);
            float distance = (pointToTest - end).sqrMagnitude;//absoluteDisplacement.sqrMagnitude;
            if(distance < shortestDistance){
                endBlock = block;
                shortestDistance = distance;
                closestPoint = pointToTest;
            }
        }
        
        int endIndex = navVolume.IndexOf(endBlock);

        NavBlock startBlock = new NavBlock();
        float shortestStartDistance = float.MaxValue;
        Vector3 closestStartPoint = Vector3.zero;
        foreach(NavBlock block in navVolume){
            block.cost = -1;
            block.costs.F = 1000000;
            block.costs.G = 1000000;
            Vector3 absoluteDisplacement = start-block.m_bounds.center;
            absoluteDisplacement = Vector3.Max(absoluteDisplacement, -absoluteDisplacement);
            absoluteDisplacement = new Vector3(
                Mathf.Clamp(absoluteDisplacement.x - block.m_bounds.extents.x/2f, 0, 10000f),
                Mathf.Clamp(absoluteDisplacement.y - block.m_bounds.extents.y/2f, 0, 10000f),
                Mathf.Clamp(absoluteDisplacement.z - block.m_bounds.extents.z/2f, 0, 10000f));
            Vector3 pointToTest = block.m_bounds.ClosestPoint(start);
            float distance = (pointToTest - start).sqrMagnitude;//absoluteDisplacement.sqrMagnitude;
            if(distance < shortestStartDistance){
                startBlock = block;
                shortestStartDistance = distance;
                closestStartPoint = pointToTest;
            }
        }
        int startIndex = navVolume.IndexOf(startBlock);
        //print("Start Block Index: " + startIndex);
        //print("End Block Index: " + endIndex);
       
        //Calculate the heuristic distance
        foreach(NavBlock _block in navVolume){
            Vector3 displacement = _block.m_bounds.ClosestPoint(end) - end;
            displacement = Vector3.Max(displacement, -displacement);
            _block.costs.H = displacement.x + displacement.y + displacement.z;

            
        }

        //Iterate through the list, trying to minimize distance
        //https://www.raywenderlich.com/3016-introduction-to-a-pathfinding
        currentBlockIndex = startIndex;
        navVolume[startIndex].costs.G = 0;
        navVolume[startIndex].costs.F = navVolume[startIndex].costs.H;

        navVolume[0].closestPoint = navVolume[startIndex].m_bounds.ClosestPoint(start);//m_bounds.center;
        closedList.Add(startIndex);
        int breakCounter = 1000;
       
        while(currentBlockIndex != endIndex){
            
            breakCounter --;
            if(breakCounter < 0){
                break;
            }

            foreach(int _link in navVolume[currentBlockIndex].links){
                if(closedList.Contains(_link)){
                    continue;
                }
                if(!openList.Contains(_link)){
                    openList.Add(_link);
                
                
                    //Calculate F score for each link block
                    Vector3 closestPointOnBounds = navVolume[_link].m_bounds.ClosestPoint(navVolume[currentBlockIndex].closestPoint); //navVolume[currentBlockIndex].closestPoint
                    navVolume[_link].costs.G = (closestPointOnBounds - navVolume[currentBlockIndex].closestPoint).magnitude + navVolume[currentBlockIndex].costs.G + 1;
                    navVolume[_link].costs.F = navVolume[_link].costs.G + navVolume[_link].costs.H;
                    navVolume[_link].closestPoint = closestPointOnBounds;
                    //plannedRoute.Add(closestPointOnBounds);
                }
            }

            //string listDump = "";
            //Sort the list, calculate the lowest length set that to be current block index, set closestPoint 
            openList.Sort((x,y) => navVolume[x].costs.F.CompareTo(navVolume[y].costs.F));
            //foreach(int item in openList){
            //    listDump += "[" + item.ToString() + " " + (navVolume[item].costs.F).ToString() + "]` ";
            //}
            

            if(openList.Count < 1){
                break;
            }
            currentBlockIndex = openList[0];
            
            /*if(closedList.Contains(currentBlockIndex)){
                print("Error");
                break;
            }*/
            closedList.Add(currentBlockIndex);
            openList.Remove(currentBlockIndex);
            

            
        }
        currentBlockIndex = endIndex;
        plannedRoute.Add(end);
        breakCounter = 10000;
       
        while(breakCounter > 0 && currentBlockIndex != startIndex){
            int bestLink = -1;
            float lowestGCost = 1000000;
            if(currentBlockIndex < 0 || currentBlockIndex >= navVolume.Count){
                Debug.LogError("CurrentBlockIndex: " + currentBlockIndex);
            }
            
            foreach(int _link in navVolume[currentBlockIndex].links){
                //listDump += currentBlockIndex.ToString() + ": [" + _link.ToString() + " " + (navVolume[_link].costs.G).ToString() + "]` ";
                if(navVolume[_link].costs.G < lowestGCost){
                    lowestGCost = navVolume[_link].costs.G;
                    bestLink = _link;
                }
            }
            //print(listDump);
           
            currentBlockIndex = bestLink;
            if(bestLink < 0){
                print("Negative Value");
                break;
            }

            plannedRoute.Add(navVolume[bestLink].closestPoint);
            breakCounter --;
        }
        
        //Reverse the route so it goes from start to end
        //Remove the start point from the route
        plannedRoute.Reverse();
        if(plannedRoute.Count > 0)
            plannedRoute.RemoveAt(0);

        //print("Finished calculating link costs");
        
        
        

        if(this.gameObject.GetComponent<LineRenderer>() == null){
            this.gameObject.AddComponent<LineRenderer>();
        }
        this.gameObject.GetComponent<LineRenderer>().enabled = false;
        //this.gameObject.GetComponent<LineRenderer>().positionCount = plannedRoute.Count;
        //this.gameObject.GetComponent<LineRenderer>().SetPositions(plannedRoute.ToArray());
        
    }
    
    

    bool isValidIndex(int x, int y, int z){
        if(x<0 || y < 0 || z<0){
            return false;
        }
        if(x >= size || y >= size || z >= size){
            return false;
        }
        return true;
    }
    /*void GetNextWaypoint(int x, int y, int z, Vector3 end, int length, int triesLeft, Queue<Vector3> currentRoute){
        if(length > currentShortestLength){
            return;
        }
       
        Vector3 currentPosition = this.transform.position - new Vector3(x,y,z) * scale;
        int indexRoute = plannedRoute.IndexOf(currentPosition);
        if(currentRoute.Contains(currentPosition) || (indexRoute <= length && indexRoute > -1)){
            return;
        }
        currentRoute.Enqueue(currentPosition);
        if(triesLeft < 0){
            if((end-currentPosition).sqrMagnitude < currentClosestPosition){
                currentClosestPosition = (end-currentPosition).sqrMagnitude;
                //currentShortestLength = length;
                plannedRoute.Clear();
                plannedRoute.AddRange(currentRoute);
            }
            return;
        }
        Vector3 absoluteDisplacement = (end-currentPosition);
        absoluteDisplacement = Vector3.Max(absoluteDisplacement, -absoluteDisplacement);
        if(absoluteDisplacement.x < scale && absoluteDisplacement.y < scale && absoluteDisplacement.z < scale){
            currentShortestLength = length;
            currentClosestPosition = (end-currentPosition).sqrMagnitude;
            plannedRoute.Clear();
            plannedRoute.AddRange(currentRoute);
            return;
        }

        if(isValidIndex(x + 1,y,z) && mesh[x+1, y, z]){
            GetNextWaypoint(x + 1, y, z, end, length + 1, triesLeft - 1, new Queue<Vector3>(currentRoute));
        }
        if(isValidIndex(x,y+1,z) && mesh[x, y+1, z]){
            GetNextWaypoint(x, y+1, z, end, length + 1, triesLeft - 1, new Queue<Vector3>(currentRoute));
        }
        if(isValidIndex(x,y,z+1) && mesh[x, y, z+1]){
            GetNextWaypoint(x, y, z+1, end, length + 1, triesLeft - 1, new Queue<Vector3>(currentRoute));
        }
        if(isValidIndex(x - 1,y,z) && mesh[x-1, y, z]){
            GetNextWaypoint(x-1, y, z, end, length + 1, triesLeft - 1, new Queue<Vector3>(currentRoute));
        }
        if(isValidIndex(x,y-1,z) && mesh[x, y-1, z]){
            GetNextWaypoint(x, y-1, z, end, length + 1, triesLeft - 1, new Queue<Vector3>(currentRoute));
        }
        if(isValidIndex(x,y,z-1) && mesh[x, y, z-1]){
            GetNextWaypoint(x, y, z-1, end, length + 1, triesLeft - 1, new Queue<Vector3>(currentRoute));
        }
        

    }*/

    // Update is called once per frame
    void Update()
    {
        if(buildCubemesh){
            buildCubemesh = false;
            numCubes = 0;
            while(transform.childCount > 0){
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            int currentBlockIndex = 0;
            navVolume.Clear();
            navVolume.Add(new NavBlock());
            for(int x = 0; x < size; x++){
                for(int y = 0; y < size; y++){
                    for(int z = 0; z < size; z++){
                        if(!Physics.CheckBox(this.transform.position - scale*new Vector3(x, y, z), scale/2.0f * new Vector3(1,1,1), Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)){
                            
                            
                            
                            mesh[x,y,z] = true;
                            Vector3 extents = navVolume[currentBlockIndex].m_bounds.extents;
                            extents.x = scale/2f;
                            extents.y = scale/2f;
                            extents.z += scale/2f;
                            navVolume[currentBlockIndex].m_bounds.extents = extents;
                            navVolume[currentBlockIndex].m_bounds.center = this.transform.position - new Vector3(scale*x, scale*y, scale*z - extents.z);
                            numCubes ++;
                        }
                        else{
                            navVolume.Add(new NavBlock());
                            currentBlockIndex ++;
                        }

                    }
                    navVolume.Add(new NavBlock());
                    currentBlockIndex ++;
                }
                navVolume.Add(new NavBlock());
                currentBlockIndex ++;
            }
            navVolume.RemoveAll(block => block.m_bounds.extents.x == 0);
            foreach(NavBlock block in navVolume){
                /*GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.GetComponent<MeshRenderer>().sharedMaterial = cubeMaterial;
                DestroyImmediate(box.GetComponent<BoxCollider>());
                                
                box.transform.position = block.m_bounds.center;
                box.transform.localScale = new Vector3(block.m_bounds.size.x, block.m_bounds.size.y, block.m_bounds.size.z);
                box.transform.SetParent(this.transform);*/

                foreach(NavBlock neighbour in navVolume.FindAll(otherBlock => 
                (Mathf.Abs(otherBlock.m_bounds.center.x- block.m_bounds.center.x) < scale*1.1f &&
                Mathf.Abs(otherBlock.m_bounds.center.y- block.m_bounds.center.y) < scale*1.1f &&
                Mathf.Abs(otherBlock.m_bounds.center.z- block.m_bounds.center.z) < block.m_bounds.extents.z + otherBlock.m_bounds.extents.z - 0.9f))){
                    if(block != neighbour){// && (new Vector3(block.m_bounds.center.x, block.m_bounds.center.y) - new Vector3(neighbour.m_bounds.center.x, neighbour.m_bounds.center.y)).sqrMagnitude < 2f * scale * scale){
                        block.links.Add(navVolume.IndexOf(neighbour));
                    }
                }

            }
                
            

            
        }

        if(findRoute){
            //findRoute = false;
            if(Time.frameCount % 10 == 0)
                FindRoute(this.transform.position, endTransform.transform.position);
        }

        if(visualize && !visualizeChecked){
            visualizeChecked = true;

            while(transform.childCount > 0){
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            foreach(NavBlock block in navVolume){
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plane.transform.position = block.m_bounds.center;
                plane.transform.localScale = block.m_bounds.size *0.8f;
                plane.GetComponent<MeshRenderer>().material = cubeMaterial;
                
                plane.transform.SetParent(this.transform);
            }
            
        }
        if(!visualize && visualizeChecked){
            visualizeChecked = false;

            while(transform.childCount > 0){
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            
        }
    }
}
