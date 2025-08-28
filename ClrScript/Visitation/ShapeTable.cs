using ClrScript.Elements;
using ClrScript.Runtime.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClrScript.Visitation
{
    class ShapeTable
    {
        readonly Dictionary<Element, ShapeInfo> _shapeInfoByElement 
            = new Dictionary<Element, ShapeInfo>();

        public ShapeInfo GetShape(Element element)
        {
            return _shapeInfoByElement.GetValueOrDefault(element);
        }

        public void SetShape(Element element, ShapeInfo shapeInfo, bool overrid = false)
        {
            if (!overrid && _shapeInfoByElement.ContainsKey(element) && shapeInfo != null)
            {
                throw new Exception("Element already has shape defined.");
            }

            _shapeInfoByElement[element] = shapeInfo;
        }

        public ShapeInfo DeriveShape(Element element1, Element element2)
        {
            return DeriveShape(GetShape(element1), GetShape(element2));
        }

        public ShapeInfo DeriveShape(ShapeInfo shapeInfo1, ShapeInfo shapeInfo2)
        {
            if (shapeInfo1 == null || shapeInfo2 == null)
            {
                return null; //unknown shape
            }

            if (shapeInfo1 is BasicShapeInfo b1
                && shapeInfo2 is BasicShapeInfo b2
                && b1.InferredType == b2.InferredType)
            {
                return shapeInfo1;
            }

            if (shapeInfo1 is ClrArrayObjectShapeInfo a1
                && shapeInfo2 is ClrArrayObjectShapeInfo a2)
            {
                return shapeInfo1;
            }

            if (shapeInfo1 is ClrScriptObjectShapeInfo o1
                && shapeInfo2 is ClrScriptObjectShapeInfo o2)
            {
                return shapeInfo1;
            }

            return null; // unknown shape
        }

        public void SetUnknownShape(Element element)
        {
            _shapeInfoByElement[element] = null;
        }
    }

    abstract class ShapeInfo
    {
        public abstract Type InferredType { get; }
    }

    class BasicShapeInfo : ShapeInfo
    {
        public override Type InferredType { get; }

        public BasicShapeInfo(Type inferredType)
        {
            InferredType = inferredType;
        }
    }

    class ClrScriptObjectShapeInfo : ShapeInfo
    {
        public override Type InferredType => typeof(ClrScriptObject);

        public Type GeneratedClrScriptObjType { get; set; }

        // This is on purpose mutable dictionary, because it translates to reference tracking of
        // objects
        // i.e this can handle
        // var obj = { name: "bob" };
        // var obj2 = obj;
        // obj.name = 12; // name gets updated to unknown shape
        // // ^ The shape for obj and obj2 is the same shape instance, so property modifications go for each

        public Dictionary<string, ShapeInfo> ShapeInfoByPropName { get; }

        public ClrScriptObjectShapeInfo(Dictionary<string, ShapeInfo> shapeInfoByPropName)
        {
            ShapeInfoByPropName = shapeInfoByPropName;
        }
    }

    class ClrArrayObjectShapeInfo : ShapeInfo
    {
        public override Type InferredType => typeof(ClrScriptArray);

        public ShapeInfo ArrayShape { get; }
    }
}
