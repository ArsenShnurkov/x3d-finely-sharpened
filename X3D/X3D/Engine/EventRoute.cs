namespace X3D.Engine
{
    public class EventRoute
    {
        #region Constructors

        public EventRoute(ROUTE route, SceneGraphNode from, SceneGraphNode to)
        {
            Route = route;
            From = from;
            To = to;
        }

        #endregion

        #region Public Fields

        public ROUTE Route;
        public SceneGraphNode From;
        public SceneGraphNode To;

        #endregion

        #region Public Methods

        public void Subscribe(ref activeRouteDelegate eventCascade)
        {
            eventCascade += () =>
            {
                string error;

                if (ExecuteEvent(out error))
                {
                }
            };
        }

        public bool ExecuteEvent(out string error)
        {
            var executed = false;
            object fromValue;
            object toValue;

            error = string.Empty;


            // EXECUTE event
            fromValue = From.getAttribute(Route.fromField);
            toValue = To.getAttribute(Route.toField);

            if (!toValue.Equals(fromValue))
            {
                // Different field access requirements depending on node

                if (To.GetType() == typeof(Script))
                {
                    // Informally: All events passed though Script node are redirected 
                    // to fields that map to variables or functions defined in the compiled Script 
                    // or as global variables defined in other scripts.

                    // Formally: Update Script field-node-children to reflect new value changes,
                    // the fields in turn must update variables in the script node to new values.
                }
                else if (To.GetType() == typeof(ProtoInstance))
                {
                    // Informally: All events passed through ProtoInstance must redirect 
                    // to the first-child of the associated ProtoDeclare.

                    // Formally: Update 
                }
                else
                {
                    To.setAttribute(Route.toField, fromValue);
                }
            }

            executed = true;

            return executed;
        }

        public override string ToString()
        {
            return string.Format("event route from node {0}<DEF=\"{4}\">.{2} to node {1}<DEF\"{5}\">.{3}",
                From, To,
                Route.fromField, Route.toField,
                From.DEF, To.DEF);
        }

        #endregion
    }
}