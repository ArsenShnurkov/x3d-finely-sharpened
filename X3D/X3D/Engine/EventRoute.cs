using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Engine
{
    public class EventRoute
    {
        public ROUTE Route;
        public SceneGraphNode From;
        public SceneGraphNode To;

        public EventRoute(ROUTE route, SceneGraphNode from, SceneGraphNode to)
        {
            this.Route = route;
            this.From = from;
            this.To = to;
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
                this.To.setAttribute(this.Route.toField, fromValue);
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
    }
}
