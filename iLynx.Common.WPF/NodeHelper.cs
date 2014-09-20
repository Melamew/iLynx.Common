using System.Collections.Generic;
using iLynx.Common.Interfaces;
using iLynx.Common.WPF.Controls;

namespace iLynx.Common.WPF
{
    public static class NodeHelper
    {
        public static object RunCalculation(this Node node)
        {
            object result;
            return !RunCalculation(node, out result) ? null : result;
        }

        public static bool RunCalculation(Node node, out object result)
        {
            result = null;
            if (null == node) return false;
            var content = node.Content;
            if (null == content) return false;
            var calculator = node.Content as INode;
            if (null == calculator) return false;
            var inputs = new List<object>();
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var inputNode in node.GetConnectedInputs())
// ReSharper restore LoopCanBeConvertedToQuery
            {
                object inputResult;
                if (!RunCalculation(inputNode, out inputResult)) continue;
                inputs.Add(inputResult);
            }
            result = calculator.Compute(inputs.ToArray());
            return true;
        }
    }
}
