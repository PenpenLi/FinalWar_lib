using bt;
using System.Collections.Generic;

namespace FinalWar
{
    internal class MergePosListNode : ConditionNode<Battle, Hero, AiData>
    {
        public override bool Enter(Battle _t, Hero _u, AiData _v)
        {
            Dictionary<string, List<int>>.ValueCollection.Enumerator enumerator = _v.posListDic.Values.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _v.posList.InsertRange(_v.posList.Count, enumerator.Current);
            }

            return true;
        }
    }
}
