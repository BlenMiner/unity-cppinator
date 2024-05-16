#include <unityengine.h>
#include <unityinterop.h>
#include <Vector2.h>
#include <Vector3.h>
#include <string>
#include <pthread.h>
#include <vector>

typedef void (*MeshCallabck)(int reqId, int vertCount, int idxCount, const Vector3* verts, const Vector3* normals, const Vector2* uvs, const int* tris);

struct TestStruct
{
    int a;
    int b;
    char* c;
};

MANAGED_EXPORT(int, someExtertedFunction, TestStruct, val)
{
    return val.a;
}

struct GenerateMeshArgs  
{
    int reqId;
    int seed;
    MeshCallabck cb;
};

void* doWork(void* args)
{
    GenerateMeshArgs* meshArgs = (GenerateMeshArgs*)args;

    auto verts = new std::vector<Vector3>();
    auto normals = new std::vector<Vector3>();
    auto uvs = new std::vector<Vector2>();
    auto tris = new std::vector<int>();

    verts->push_back(Vector3(0, 0, 0));
    verts->push_back(Vector3(1, 0, 0));
    verts->push_back(Vector3(0, 1, 0));
    verts->push_back(Vector3(1, 1, 0));

    normals->push_back(Vector3(0, 0, 1));
    normals->push_back(Vector3(0, 0, 1));
    normals->push_back(Vector3(0, 0, 1));
    normals->push_back(Vector3(0, 0, 1));

    uvs->push_back(Vector2(0, 0));
    uvs->push_back(Vector2(1, 0));
    uvs->push_back(Vector2(0, 1));
    uvs->push_back(Vector2(1, 1));

    tris->push_back(0);
    tris->push_back(1);
    tris->push_back(2);
    tris->push_back(1);
    tris->push_back(3);
    tris->push_back(2);

    DebugLog("Generated mesh with " + std::to_string(verts->size()) + " verts and " + std::to_string(tris->size()) + " tris");

    QueueToMainThread([meshArgs, verts, normals, uvs, tris]()
    {
        meshArgs->cb(meshArgs->reqId, verts->size(), tris->size(), verts->data(), normals->data(), uvs->data(), tris->data());

        delete verts;
        delete normals;
        delete uvs;
        delete tris;
        delete meshArgs;
    });

    pthread_exit(NULL);
    return NULL;
}
 
EXPORT(void) GenerateMeshExample(int reqId, int seed, MeshCallabck cb)
{
    GenerateMeshArgs* args = new GenerateMeshArgs();

    args->reqId = reqId;
    args->seed = seed;
    args->cb = cb;

    pthread_t thread;

    pthread_create(&thread, NULL, doWork, args);
}
