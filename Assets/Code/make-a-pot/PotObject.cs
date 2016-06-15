using UnityEngine;
using System;

public class PotObject : MonoBehaviour {

    public uint layersCount = 100;    
    public float potHeight = 4.0f;
    public float maxRadius = 2.0f;
    public float minRadius = 0.2f;
    public float startRadius = 1.3f;

    private uint verticesPerCircle = 4 << 4;
    
    private uint VERTICES_CNT;
    private uint TRIANGLES_CNT;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private float[] radiuses;
    private float[] curRadiuses; // current mesh radiuses
    private float transformSpeed = 1.0f;
    private float angleBetweenVertices;
    private float layerHeight;

    private bool needTransform = false;    
    private Vector3[] currentVertices;

    // Use this for initialization
    void Start () 
    {
        radiuses = new float[layersCount];
        curRadiuses = new float[layersCount];
        angleBetweenVertices = 2 * Mathf.PI / verticesPerCircle;
        VERTICES_CNT = /* each layer circle */ layersCount * verticesPerCircle + /* top and bottom centers */ 2 + /* extra vertex for each layer for correct UV mapping */ layersCount + /* extra vertices for top and bottom */ 2 * (verticesPerCircle + 1);
        TRIANGLES_CNT = /* top and bottom */ 2 * verticesPerCircle * 3 + /* sides, 2 triangle per vertex */ (layersCount - 1) * verticesPerCircle * 2 * 3;
        layerHeight = potHeight / layersCount;
        
        for (uint i = 0; i < layersCount; ++i)
        {
            curRadiuses[i] = radiuses[i] = startRadius;
        }
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        recreateMesh();
    }
	
	// Update is called once per frame
	void Update ()
    {
        bool needMeshUpdate = false;
        int i;
        if (Input.GetKey("r")) {
			for (i = 0; i < layersCount; ++i)
            {
                radiuses[i] = startRadius;
            }
            recreateMesh();
            needMeshUpdate = false;
		} 
        else if (needTransform) 
        {
            bool hasDiff = false;
            for (i = 0; i < layersCount; ++i) {
                if (curRadiuses[i] != radiuses[i]) {
                    float delta = radiuses[i] - curRadiuses[i];
                    var sign = delta / Math.Abs(delta);
                    curRadiuses[i] += sign * transformSpeed * Time.deltaTime;
                    if (sign == -1) { if (curRadiuses[i] < radiuses[i]) curRadiuses[i] = radiuses[i]; }
                    else { if (curRadiuses[i] > radiuses[i]) curRadiuses[i] = radiuses[i]; } 
                    recreateLayer(i, curRadiuses[i]);
                    if (!needMeshUpdate) needMeshUpdate = true;
                    if (!hasDiff) hasDiff = true;
                }
            }
            if (!hasDiff) needTransform = false;
        }
        
	    if(needMeshUpdate)
        {
            meshFilter.mesh.vertices = currentVertices;
            meshCollider.sharedMesh = meshFilter.mesh;
            needMeshUpdate = false;
        }
	}

    void OnTriggerStay(Collider other)
    {
        int layer = (int)Math.Floor((other.transform.position.y - transform.position.y) / layerHeight) + 1;
        if (layer >= layersCount) { layer = (int)(layersCount); }
        
        float impactDepth ;
        float y;
        var mBounds = other.bounds;
        int impactLayers = (int)(Math.Ceiling(mBounds.extents.y / layerHeight) / 2.0);
        
        var layerMin = (uint)Math.Max(layer - impactLayers, 0);
        var layerMax = (uint)Math.Min(layer + impactLayers, layersCount - 1);
        for (uint j = layerMin; j <= layerMax; ++j)
        {
            y = (float)j * potHeight / (float)(layersCount - 1);
            
            var potPoint = transform.position; potPoint.y = y + transform.position.y;
            var colliderPoint = other.ClosestPointOnBounds(potPoint);
            impactDepth = radiuses[j] - Vector3.Distance(potPoint, colliderPoint);
            
            if (impactDepth > 0) radiuses[j] -= impactDepth;
            if (radiuses[j] < minRadius) {
                bool hasUpper = false;
                for (uint k = j + 1; k <= layersCount - 1; ++k)
                {
                    if (radiuses[k] > minRadius) { hasUpper = true; break; }
                }
                if (hasUpper) radiuses[j] = minRadius;
                else {
                    radiuses[j] = 0;
                    for (uint k = j + 1; k <= layersCount - 1; ++k)
                    {
                        if (radiuses[k] > radiuses[j]) {
                            radiuses[k] = radiuses[j];
                        }
                    }
                    
                }
            }
        }
        needTransform = true;
    }
    
    void OnTriggerExit(Collider other)
    {
        StopPull(); // stops radius changes
    }
    
    void recreateLayer(int j, float radius)
    {
        uint i;
        float _angle, _x, _z, y;
        long offset = 1 + (verticesPerCircle + 1) * (j + 1);
        
        y = (float)j * potHeight / (float)(layersCount - 1);
        for (i = 0; i < (verticesPerCircle + 1); ++i)
        {
            _angle = i * angleBetweenVertices;
            _x = Mathf.Cos(_angle) * radius;
            _z = Mathf.Sin(_angle) * radius;
            currentVertices[offset + i] = new Vector3(_x, y, _z);
        }
        
        if (j == 0 || j == layersCount - 1)
        {
            offset = j == 0 ? 1 : (1 + (verticesPerCircle + 1) * (j + 2));
            for (i = 0; i < (verticesPerCircle + 1); ++i)
            {
                _angle = i * angleBetweenVertices;
                _x = Mathf.Cos(_angle) * radius;
                _z = Mathf.Sin(_angle) * radius;
                currentVertices[offset + i] = new Vector3(_x, y, _z);
            }
        }
    }
    
    public void PullLayer(Vector3 pos, float height)
    {
        StopPull();
        int layer = layerFromY(pos.y);
        int impactLayers = (int)(Math.Ceiling(height / layerHeight) / 2.0);
        if (layer != -1) {
            var potPoint = transform.position; potPoint.y = pos.y + transform.position.y;
            var layerMin = (uint)Math.Max(layer - impactLayers, 0);
            var layerMax = (uint)Math.Min(layer + impactLayers, layersCount - 1);
            for (uint j = layerMin; j <= layerMax; ++j)
            {
                var dist = Vector3.Distance(potPoint, pos);
                radiuses[j] = dist;
                if (radiuses[j] > maxRadius) radiuses[j] = maxRadius;
            }
        }
        needTransform = true;
    }
    public void StopPull()
    {
        for (uint i = 0; i < layersCount; ++i)
        {
            radiuses[i] = curRadiuses[i];
        }
        needTransform = false;
    }
    
    private int layerFromY(float y, bool returnAnyway = false)
    {
        int layer = (int)Math.Floor((y - transform.position.y) / layerHeight) + 1;
        if (layer > layersCount - 1) {
            return returnAnyway ? (int)(layersCount - 1) : -1;
        } else if (layer < 0) {
            return returnAnyway ? 0 : -1;
        }
        return layer;
    }

    /**
     *
     */
    private void recreateMesh()
    {
        uint layer, offset = 0, triOffset = 0, layerVertices;
        var aMesh = new Mesh();

        currentVertices = new Vector3[VERTICES_CNT];
        var triangles = new int[TRIANGLES_CNT];
        var normals = new Vector3[VERTICES_CNT];
        var uv = new Vector2[VERTICES_CNT];
        float _y;

        currentVertices[offset] = new Vector3(0.0f, 0.0f, 0.0f); // Center vertex for bottom
        normals[offset] = Vector3.down;
        uv[offset] = new Vector2(0.1f, 0.3f);
        ++offset;
        currentVertices[VERTICES_CNT - 1] = new Vector3(0.0f, potHeight, 0.0f); // Center vertex for top
        normals[VERTICES_CNT - 1] = Vector3.up;
        uv[VERTICES_CNT - 1] = new Vector2(0.1f, 0.1f);
        
        // Create bottom layer
        layerVertices = createLayer(ref currentVertices, ref normals, ref uv, offset, 0.0f, radiuses[0], 0.0f, 1.0f, Vector3.down);
        createCoverUV(ref uv, offset, radiuses[0], false);
        triOffset += createBottomTriangles(ref triangles, triOffset, offset, 0, true);
        offset += layerVertices;
        
        // Create circles for each layer
        for (layer = 0; layer < layersCount; ++layer)
        {
            _y = (float)layer * potHeight / (float)(layersCount - 1);
            layerVertices = createLayer(ref currentVertices, ref normals, ref uv, offset, _y, radiuses[layer], 0.0f, (float)layer / (float)(layersCount - 1));
            if (layer > 0)
            {
                triOffset += createSideTriangles(ref triangles, triOffset, offset);
            }
            offset += layerVertices;
        }

        // Create top layer
        layerVertices = createLayer(ref currentVertices, ref normals, ref uv, offset, potHeight, radiuses[layersCount - 1], 0.0f, 1.0f, Vector3.up);
        createCoverUV(ref uv, offset, radiuses[0], true);
        triOffset += createBottomTriangles(ref triangles, triOffset, offset, VERTICES_CNT - 1, false);
        offset += layerVertices;

        aMesh.vertices = currentVertices;
        aMesh.triangles = triangles;
        aMesh.normals = normals;
        aMesh.uv = uv;
        meshFilter.mesh = aMesh;
        meshCollider.sharedMesh = aMesh;
    }

    /**
     * @fn createLayer
     * Creates vertices for the layer
     */
    private uint createLayer(ref Vector3[] vertices, ref Vector3[] normals, ref Vector2[] uv, uint offset, float y, float radius, float normalVectorZ, float V, Nullable<Vector3> normalOverride = null)
    {
        float _angle, _x, _z;
        uint vertexInd = offset;
        for (uint i = 0; i <= verticesPerCircle; ++i) // create circle vertices
        {
            _angle = i * angleBetweenVertices;
            _x = Mathf.Cos(_angle) * radius;
            _z = Mathf.Sin(_angle) * radius;
            vertices[vertexInd] = new Vector3(_x, y, _z);

            if (normalOverride.HasValue)
                normals[vertexInd] = normalOverride.Value;
            else {
                normals[vertexInd] = new Vector3(Mathf.Cos(_angle), normalVectorZ, Mathf.Sin(_angle));
                uv[vertexInd] = new Vector2((float)i / (float)verticesPerCircle /* * 0.8f + 0.2f */, V);
            }
            
            ++vertexInd;
        }
        // extra vertex for UV. Equals to first layer vertex
        return verticesPerCircle + 1;
    }
    
    private void createCoverUV(ref Vector2[] uv, uint offset, float radius, bool isTop = false)
    {
        float _angle, _u, _v;
        uint vertexInd = offset;
        for (uint i = 0; i <= verticesPerCircle; ++i) // create circle vertices
        {
            _angle = i * angleBetweenVertices;
            _u = Mathf.Cos(_angle) * 0.1f + 0.1f;
            _v = Mathf.Sin(_angle) * 0.1f + 0.1f;
            if (!isTop) _v += 0.2f;
            
            uv[vertexInd] = new Vector2(_u, _v);
            ++vertexInd;
        }
    }
    
    /**
     * @fn createBottomTriangles
     * Creates triangles for bottom of the pot
     */
    private uint createBottomTriangles(ref int[] triangles, uint triOffset, uint firstVertexOfLayer, uint centerVertex, bool clockwiseDirection = false)
    {
        uint offset = triOffset;
        for (uint i = 0; i < verticesPerCircle; ++i)
        {
            if (clockwiseDirection)
            {
                triangles[offset + 1] = (int)(firstVertexOfLayer + i);
                triangles[offset    ] = (int)centerVertex;
                triangles[offset + 2] = (int)(firstVertexOfLayer + i + 1);
            } else
            {
                triangles[offset + 1] = (int)(firstVertexOfLayer + i + 1);
                triangles[offset    ] = (int)centerVertex;
                triangles[offset + 2] = (int)(firstVertexOfLayer + i);
            }
            offset += 3;
        }
        return verticesPerCircle * 3;
    }
    
    /**
     * @fn createSideTriangles
     * Creates triangles for rendering pot sides
     */
    private uint createSideTriangles(ref int[] triangles, uint offset, uint firstVertexOfLayer)
    {
        uint triOffset = offset;
        for (uint i = 0; i < verticesPerCircle; ++i)
        {
            triangles[triOffset + 1] = (int)(firstVertexOfLayer + i + 1);
            triangles[triOffset    ] = (int)(firstVertexOfLayer + i);
            triangles[triOffset + 2] = (int)(firstVertexOfLayer - (verticesPerCircle + 1) + i);
            triOffset += 3;
            triangles[triOffset + 1] = (int)(firstVertexOfLayer - (verticesPerCircle + 1) + i + 1);
            triangles[triOffset    ] = (int)(firstVertexOfLayer + i + 1);
            triangles[triOffset + 2] = (int)(firstVertexOfLayer - (verticesPerCircle + 1) + i);
            triOffset += 3;
        }
        return 2 * verticesPerCircle * 3;
    }
}
