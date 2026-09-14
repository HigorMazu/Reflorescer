using Godot;
using System.Collections.Generic;

namespace Joguim.Utilities
{
    public partial class ObjectPool : Node
    {
        private PackedScene _scene;
        private Queue<Node> _pool = new();
        private Node _parent;
        private int _maxSize;

        public ObjectPool(PackedScene scene, Node parent, int initialSize = 10, int maxSize = 100)
        {
            _scene = scene;
            _parent = parent;
            _maxSize = maxSize;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = _scene.Instantiate<Node>();
                if (obj is CanvasItem ci) ci.Visible = false;
                obj.ProcessMode = ProcessModeEnum.Disabled;
                _parent.AddChild(obj);
                _pool.Enqueue(obj);
            }
        }

        public Node Get()
        {
            Node obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else if (_pool.Count < _maxSize)
            {
                obj = _scene.Instantiate<Node>();
                _parent.AddChild(obj);
            }
            else
            {
                return null;
            }

            if (obj is CanvasItem ci) ci.Visible = true;
            obj.ProcessMode = ProcessModeEnum.Inherit;
            return obj;
        }

        public void Return(Node obj)
        {
            if (obj == null) return;

            if (obj is CanvasItem ci) ci.Visible = false;
            obj.ProcessMode = ProcessModeEnum.Disabled;
            _pool.Enqueue(obj);
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (GodotObject.IsInstanceValid(obj))
                {
                    obj.QueueFree();
                }
            }
        }
    }
}
