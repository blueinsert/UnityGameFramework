using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bluebean.UGFramework.GamePlay
{
    public interface IRollbackAble
    {
        void TakeSnapShot(SnapShotWriter writer);
        void RollBackTo(SnapShotReader reader);
    }
}
