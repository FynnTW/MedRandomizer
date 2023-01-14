
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mapStuff;
using MedRandomizer;
using Realistic;

class MainFile
{
    static void Main(string[] args)
    {
        createNew();
        //createOld();
    }



    static void createNew()
    {
        RealMap.mapWidth = 510;
        RealMap.mapHeight = 510;
        RealMap.landplateCount = 4;
        RealMap.seaplateCount = 0;
        RealMap.blurLength = 3; //20
        RealMap.blurWeight = 4; //50
        RealMap.minLandFillCont = 20; //20
        RealMap.minLandFillSea = 3; //50
        RealMap.createMap();
    }
    static void createOld()
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
