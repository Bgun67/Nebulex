using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface_Pathfinding : MonoBehaviour
{
    public MeshFilter[] meshFilters;
    Mesh mesh;
    MeshCollider meshCollider;
    MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    public struct Path{
        public float distance;
        public int startIndex;
        public int endIndex;
    }
    List<Path> paths = new List<Path>();
    List<int> triangles = new List<int>();
    public float threshold = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        meshCollider = this.GetComponent<MeshCollider>();
        meshFilter = this.GetComponent<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].mesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;

            i++;
        }
        mesh = new Mesh();
        print(mesh.isReadable);
        mesh.CombineMeshes(combine,true);
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation 
                                    | MeshColliderCookingOptions.EnableMeshCleaning 
                                    | MeshColliderCookingOptions.UseFastMidphase 
                                    | MeshColliderCookingOptions.WeldColocatedVertices;
        
        
        mesh.GetVertices(vertices);
        mesh.GetNormals(normals);
        mesh.GetTriangles(triangles, 0);
        
        RaycastHit hit;
        int j = 0;
        while(j < vertices.Count){
            vertices[j] = vertices[j] + normals[j] * 0.2f;
            j++;
        }
        j = 0;
        while(j < vertices.Count){
            vertices.RemoveAll(vertex => Vector3.SqrMagnitude(vertex - vertices[j]) < threshold && vertex!= vertices[j]);
            //vertices[j] = vertices[j] + normals[j] * 0.2f;
            j++;
        }
        print(vertices.Count);
        j=0;
        while(j < vertices.Count){
            
            for(int k = 0; k<vertices.Count; k++){
                if(j != k && !meshCollider.Raycast(new Ray(vertices[j], (vertices[k] - vertices[j]).normalized),out hit, 10f)){
                    Path newPath = new Path();
                    newPath.startIndex = j;
                    newPath.endIndex = k;
                    newPath.distance = hit.distance;
                    //if(newPath.distance > 0.6 && paths.FindIndex(path => path.startIndex == newPath.endIndex) < 0)
                        paths.Add(newPath);
                }
            }
            j++;
        }
        /*for(int m = 0; m<triangles.Count; m+=3){
            Path newPath1 = new Path();
            newPath1.startIndex = triangles[m];
            newPath1.endIndex = triangles[m+1];
            //newPath1.distance = hit.distance;
            paths.Add(newPath1);

            Path newPath2 = new Path();
            newPath2.startIndex = triangles[m];
            newPath2.endIndex = triangles[m+2];
            //newPath1.distance = hit.distance;
            paths.Add(newPath2);

            Path newPath3 = new Path();
            newPath3.startIndex = triangles[m+2];
            newPath3.endIndex = triangles[m+1];
            //newPath1.distance = hit.distance;
            paths.Add(newPath3);
        }*/
        
        //vertices.RemoveAll(point => Vector3.Distance(meshCollider.ClosestPoint(point), point) < 0.001f);
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < paths.Count; i++){
            Debug.DrawLine(vertices[paths[i].startIndex], vertices[paths[i].endIndex], Color.blue);
        }
        //Debug.DrawLine(meshCollider.ClosestPoint(vertices[j] + normals[j] * 1f),vertices[j] + normals[j] * 1f, Color.yellow, 30f);
        
    }
}
