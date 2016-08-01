using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine
{
    public class EventRoute
    {
        #region Public Fields

        public ROUTE Route;
        public SceneGraphNode From;
        public SceneGraphNode To;

        #endregion

        #region Constructors

        public EventRoute(ROUTE route, SceneGraphNode from, SceneGraphNode to)
        {
            this.Route = route;
            this.From = from;
            this.To = to;
        }

        #endregion

        #region Public Methods

        public void Subscribe(ref activeRouteDelegate eventCascade)
        {
            eventCascade += () =>
            {
                string error;

                if (this.ExecuteEvent(out error))
                {

                }
            };
        }

        public bool ExecuteEvent(out string error)
        {
            bool executed = false;
            object fromValue;
            object toValue;

            error = string.Empty;


            // EXECUTE event
            fromValue = this.From.getAttribute(this.Route.fromField);
            toValue = this.To.getAttribute(this.Route.toField);

            if (!toValue.Equals(fromValue))
            {
                // Different field access requirements depending on node

                if(this.To.GetType() == typeof(Script))
                {
                    // Informally: All events passed though Script node are redirected 
                    // to fields that map to variables or functions defined in the compiled Script 
                    // or as global variables defined in other scripts.

                    // Formally: Update Script field-node-children to reflect new value changes,
                    // the fields in turn must update variables in the script node to new values.

                }
                else if (this.To.GetType() == typeof(ProtoInstance))
                {
                    // Informally: All events passed through ProtoInstance must redirect 
                    // to the first-child of the associated ProtoDeclare.

                    // Formally: Update 
                }
                else
                {
                    this.To.setAttribute(this.Route.toField, fromValue);
                }
            }

            executed = true;

            return executed;
        }

        public override string ToString()
        {
            return string.Format("event route from node {0}<DEF=\"{4}\">.{2} to node {1}<DEF\"{5}\">.{3}",
                    this.From, this.To,
                    this.Route.fromField, this.Route.toField,
                    this.From.DEF, this.To.DEF);
        }

        #endregion
    }
}
