using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace iLynx.Common.Interfaces
{
    public interface IRequestRecalculate
    {
        event EventHandler RequestRecalculate;
    }

    public interface INode : IRequestRecalculate, INotifyPropertyChanged
    {
        object Compute(params object[] args);
        void OnInputConnected(INode newInput);
        void OnInputDisconnected(INode oldInput);
        void OnOutputConnected(INode newOutput);
        void OnOutputDisconnected(INode oldOutput);
        bool AutoAddInputs { get; }
        bool AutoAddOutputs { get; }
        IEnumerable<Type> Inputs { get; }
        IEnumerable<Type> Outputs { get; }
    }
}
