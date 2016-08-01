//#define DEBUG_EVENTS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace X3D.Engine
{
    public delegate void activeRouteDelegate();

    /// <summary>
    /// Implements the runtime behaviour of events between nodes of the Scene Graph.
    /// </summary>
    public class EventGraph
    {
        AutoResetEvent suspend = new AutoResetEvent(false);

        #region Private Fields

        /// <summary>
        /// The list of queued events to be fired on the next timestamp.
        /// Events build up in the queue, and processed one at a time.
        /// This ensures any cyclic ROUTE relationships can exist without causing problems.
        /// </summary>
        private Queue<EventRoute> queue = new Queue<EventRoute>();

        /// <summary>
        /// Next events that will be dispatched (carried on) to the next timestamp
        /// </summary>
        private List<EventRoute> nextDispatch = new List<EventRoute>();

        /// <summary>
        /// The Scene Graph the Event Model is derived from.
        /// </summary>
        private SceneGraph sg;

        //private BackgroundWorker bw;
        private Thread propagator;
        private bool isPropagating = false;
        //private bool suspended = false;

        
        public event activeRouteDelegate eventCascade;
        private bool cascadeReady = false;

        #endregion

        #region Constructors

        
        public EventGraph()
        {
            ThreadStart ts = new ThreadStart(() =>
            {
                while (true)
                {
                    isPropagating = true;
                    traverseEventGraph();
                    isPropagating = false;
                    
                    //suspended = true;
                    //suspend.WaitOne();
                    //suspended = false;
                }

            });
            propagator = new Thread(ts);

            //bw = new BackgroundWorker();
            //bw.WorkerSupportsCancellation = true;
            //bw.DoWork += new DoWorkEventHandler((object sender, DoWorkEventArgs e) =>
            //{
            //    isPropagating = true;
            //    traverseEventGraph();
            //    isPropagating = false;
            //    bw.CancelAsync();
            //    suspended = true;
            //});
        }

        #endregion

        #region Public Methods

        public void AssignSceneGraph(SceneGraph sceneGraph)
        {
            this.sg = sceneGraph;
        }

        /// <summary>
        /// Propagates any events that have been queued in the runtime.
        /// </summary>
        public void PropagateEvents(RenderingContext rc)
        {
            //if (!bw.IsBusy && !isPropagating)
            //{
            //    bw.RunWorkerAsync();
            //    suspended = false;
            //}

            //BUG: event model slowing down framerate when in fullscreen even though it is on another thread

            if (!propagator.IsAlive && !isPropagating)
            {
                propagator.Start();
            }

            //if (suspended && !isPropagating)
            //{
            //    suspend.Set();
            //}
        }

        /// <summary>
        /// Establish event routes between nodes and their fields in the Scene Graph
        /// </summary>
        public void CreateEventGraph()
        {
            EventRoute @event;
            SceneGraphNode fromNode;
            SceneGraphNode toNode;
            bool fHas;
            bool tHas;
            string error;

            queue.Clear();
            nextDispatch.Clear();

            foreach (ROUTE route in sg.Routes)
            {
                fromNode = sg.defUseScope[route.fromNode]; // quick node lookup by DEF attribute
                toNode = sg.defUseScope[route.toNode];

                @event = new EventRoute(route, fromNode, toNode);

                fHas = fromNode.HasAttribute(route.fromField);
                tHas = toNode.HasAttribute(route.toField);

                error = string.Empty;

                if (fHas && tHas)
                {
                    queue.Enqueue(@event);
                }
                if (!fHas)
                {
                    error += " from attribute not found";
                }

                if (!tHas)
                {
                    error += " to attribute not found";
                }

                error = error.Trim();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("->Error {1} {0}", @event.ToString(), error);
                }
            }
        }

        #endregion

        #region Private Methods



        private void traverseEventGraph()
        {
            EventRoute @event;
            EventRoute next;
            string error;
            int i;

            //TODO: only queue events where values have changed; maybe set up notification system

            if (!cascadeReady)
            {
                while (queue.Count > 0)
                {
#if DEBUG_EVENTS
                Console.WriteLine("Dequeue event");
#endif
                    @event = queue.Dequeue();
#if DEBUG_EVENTS
                Console.WriteLine("Executing {0}", @event);
#endif
                    @event.Subscribe(ref eventCascade);




                }

                cascadeReady = true;
            }

            if (cascadeReady)
            {
                if(eventCascade != null)
                    eventCascade();
            }

//            while (queue.Count > 0)
//            {
//#if DEBUG_EVENTS
//                Console.WriteLine("Dequeue event");
//#endif
//                @event = queue.Dequeue();
//#if DEBUG_EVENTS
//                Console.WriteLine("Executing {0}", @event);
//#endif
//                if (@event.ExecuteEvent(out error))
//                {
//                    nextDispatch.Add(@event);
//                }
//            }

//            queue.Clear();

//            for (i = 0; i < nextDispatch.Count; i++)
//            {
//                next = nextDispatch[i];

//#if DEBUG_EVENTS
//                Console.WriteLine("Enqueue event {0}", next);
//#endif
//                queue.Enqueue(next);
//            }
        }

        #endregion

    }
}
