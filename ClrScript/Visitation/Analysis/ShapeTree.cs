using ClrScript.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation.Analysis
{
    class ShapeTree
    {
        readonly Dictionary<Element, Shape> _shapesByElement 
            = new Dictionary<Element, Shape>();

        public IReadOnlyDictionary<Element, Shape> ShapesByElement => _shapesByElement;

        public Shape GetShape(Element element)
        {
            if (!_shapesByElement.TryGetValue(element, out var value))
            {
                throw new Exception("Shape not set.");
            }

            return value;
        }

        public void SetShape(Element element, Shape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException(nameof(shape));
            }

            _shapesByElement[element] = shape;
        }

        public IEnumerable<T> GetShapesOfType<T>()
        {
            return _shapesByElement
                .Select(p => p.Value)
                .Where(s => s is T)
                .Cast<T>();
        }
    }

    abstract class Shape
    {
    }

    class UndeterminedShape : Shape
    {
        public static UndeterminedShape Instance { get; } = new UndeterminedShape();
    }

    class ConcreteShape : Shape
    {
        public Type Type { get; }
    }

    class PointerShape : Shape
    {
        public Shape PointsTo { get; set; }
    }

    class DerivedShape : Shape
    {
        public List<Shape> Children { get; }

        public DerivedShape(List<Shape> children)
        {
            Children = children;
        }

        public static DerivedShape CreateDerivedShape(params Shape[] shapes)
        {
            return new DerivedShape(shapes.ToList());
        }

        public static DerivedShape CreateDerivedShape(IEnumerable<Shape> shapes)
        {
            return new DerivedShape(shapes.ToList());
        }
    }

    class ArrayShape : DerivedShape
    {
        public ArrayShape(List<Shape> children) : base(children)
        {
        }
    }

    class ObjectShape : Shape
    {
        public Dictionary<string, Shape> Properties { get; } = new Dictionary<string, Shape>();
    }

    abstract class MethodShape : Shape
    {
        public Shape Return { get; set; }

        public IReadOnlyList<Shape> Args { get; }

        public MethodShape(IReadOnlyList<Shape> args)
        {
            Args = args;
        }
    }

    class ExternalMethodShape : MethodShape
    {
        public ExternalMethodShape(IReadOnlyList<Shape> args) : base(args)
        {
        }
    }

    class LambdaMethodShape : MethodShape
    {
        public LambdaMethodShape(IReadOnlyList<Shape> args) : base(args)
        {
        }
    }
}
