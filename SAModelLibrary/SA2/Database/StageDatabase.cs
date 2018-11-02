using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAModelLibrary.SA2.Database
{
    public static class StageDatabase
    {
        public static IReadOnlyList<StageInfo> Stages { get; } = new List<StageInfo>()
        {
            new StageInfo("Green Forest", 3, "objLandTable0003", 0x01089260)
        };

        public static StageInfo GetStageInfo( int id )
        {
            var info = Stages.FirstOrDefault( x => x.Id == id );
            return info;
        }
    }

    public class StageInfo
    {
        public string FriendlyName { get; }

        public int Id { get; }

        public string LandTableName { get; }

        public int DeathZoneAddress { get; }

        public StageInfo(string friendlyName, int id, string landTableName, int deathZoneAddress)
        {
            FriendlyName = friendlyName;
            Id = id;
            LandTableName = landTableName;
            DeathZoneAddress = deathZoneAddress;
        }
    }
}
