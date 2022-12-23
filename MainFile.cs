
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mapStuff;

class MainFile
{
    static void Main(string[] args)
    {

        mapCreator.contWidth = 500; //500
        mapCreator.blurLength = 15; //20
        mapCreator.blurWeight = 50; //50
        mapCreator.regionCount = 199;
        int mapWidth = 510;
        int mapHeight = 510;
        int continents = 1;

        mapCreator.CreateMap(mapWidth, mapHeight, continents);
    }
}
