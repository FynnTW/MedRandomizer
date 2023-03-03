using mapStuff;
using Realistic;

internal class MainFile
{
    private static void Main(string[] args)
    {
        createNew();
        //createOld();
    }

    private static void createNew()
    {
        RealMap.mapWidth = 1021;
        RealMap.mapHeight = 1024;
        RealMap.landplateCount = 4;
        RealMap.seaplateCount = 0;
        RealMap.blurLength = 3; //20
        RealMap.blurWeight = 4; //50
        RealMap.minLandFillCont = 40; //20
        RealMap.minLandFillSea = 3; //50
        RealMap.createMap();
    }

    private static void createOld()
    {
        mapCreator.contWidth = 500; //500
        mapCreator.blurLength = 3; //20
        mapCreator.blurWeight = 10; //50
        mapCreator.regionCount = 199;
        int mapWidth = 510;
        int mapHeight = 510;
        int continents = 1;

        mapCreator.CreateMap(mapWidth, mapHeight, continents);
    }
}