using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X3D.Parser
{
    public class X3DRuntimeValidator
    {
        public static bool debug = true;

        /// <summary>
        /// Validate relationships in scene at runtime and make adjustments instantly as necessary.
        /// </summary>
        /// <returns>
        /// True, if the types of children of the current node are allowable under this type of SceneGraphNode
        /// Otherwise false, if there were any children found that arent valid relationships.
        /// </returns>
        public static bool Validate(SceneGraphNode node)
        {
            bool passed = true;
            bool warned0 = false;
            bool warned1 = false;

            node.isValid = null;

            // X3D VALIDATION

            // Test Appearance Component
            appearanceValidationConstraints(node, out passed, out warned0);

            // Test Shader Component
            shaderValidationConstraints(node, out passed, out warned1);

            //TODO: test other X3D node relationships

            node.isValid = passed;

            node.alreadyWarned = warned0 || warned1;

            return passed;
        }

        #region X3D Component Constraints

        // TODO: come up with a better way to define constraints between nodes, and act on them

        //TODO: Validate more X3D components

        private static void appearanceValidationConstraints(SceneGraphNode parent, out bool passed, out bool warned)
        {
            List<SceneGraphNode> invalid;

            passed = true;
            warned = false;

            if (typeof(X3DAppearanceNode).IsInstanceOfType(parent))
            {
                if (!parent.Children.Any(n => (typeof(X3DAppearanceChildNode).IsInstanceOfType(n))))
                {
                    warned = true;

                    if (!parent.alreadyWarned && debug) Console.Write("[Warning] {0} doesnt contain any X3DAppearanceChildNode children ", this.ToString());

                    if (!parent.alreadyWarned && debug) Console.WriteLine(parent.ErrorStringWithLineNumbers());
                }

                invalid = parent.Children.Where(n => !typeof(X3DAppearanceChildNode).IsInstanceOfType(n)).ToList();

                processInvalidNodes(parent, invalid, parent.ToString(), out passed);
            }
        }


        private static void shaderValidationConstraints(SceneGraphNode parent, out bool passed, out bool warned)
        {
            List<SceneGraphNode> invalid;

            passed = true;
            warned = false;

            if (typeof(X3DProgrammableShaderObject).IsInstanceOfType(parent))
            {
                // Only allowed to have ShaderPart, and field children

                if (!parent.Children.Any(n => (typeof(field).IsInstanceOfType(n))))
                {
                    warned = true;

                    if (!parent.alreadyWarned && debug) Console.WriteLine("[Warning] {0} doesnt contain any field children", parent.ToString());
                }

                if (typeof(ComposedShader).IsInstanceOfType(parent))
                {

                    invalid = parent.Children.Where(n => !((typeof(ShaderPart).IsInstanceOfType(n) || typeof(field).IsInstanceOfType(n)))).ToList();

                    processInvalidNodes(parent, invalid, parent.ToString(), out passed);

                    var shaderParts = parent.Children.Where(n => (typeof(ShaderPart).IsInstanceOfType(n))).Select(part => (ShaderPart)part).ToList();

                    if (!shaderParts.Any())
                    {
                        passed = false;

                        if (debug) Console.WriteLine("ComposedShader must contain ShaderPart children");

                        pruneBadRelationship(parent, referToParent: true);
                    }
                    else
                    {
                        for (int i = 0; i < shaderParts.Count(); i++)
                        {
                            var part = shaderParts[i];

                            // TODO: are URLs actually valid?

                            if (part.Children.Any())
                            {
                                passed = false;

                                if (debug) Console.WriteLine("ShaderPart must not have any children defined other than a CDATA text node");

                                pruneBadRelationship(part);
                            }

                            if (string.IsNullOrEmpty(part.url) && string.IsNullOrEmpty(part.ShaderSource.Trim()))
                            {
                                passed = false;

                                if (debug) Console.WriteLine("ShaderPart must have a url attribute or CDATA section defined");

                                pruneBadRelationship(part);
                            }
                            else if (!string.IsNullOrEmpty(part.ShaderSource) && string.IsNullOrEmpty(part.ShaderSource.Trim()))
                            {
                                passed = false;

                                if (debug) Console.WriteLine("ShaderPart must have a CDATA section properly defined");

                                pruneBadRelationship(part);
                            }
                        }
                    }
                }
                else if (typeof(PackagedShader).IsInstanceOfType(parent))
                {
                    invalid = parent.Children;

                    processInvalidNodes(parent, invalid, parent.ToString(), out passed, noChildrenAllowed: true);
                }
                else if (typeof(ShaderProgram).IsInstanceOfType(parent))
                {
                    invalid = parent.Children;

                    processInvalidNodes(parent, invalid, parent.ToString(), out passed, noChildrenAllowed: true);
                }
            }
            else if (typeof(ShaderPart).IsInstanceOfType(parent))
            {
                invalid = parent.Children;

                processInvalidNodes(parent, invalid, parent.ToString(), out passed, noChildrenAllowed: true);
            }
            else if (typeof(ProgramShader).IsInstanceOfType(parent))
            {
                invalid = parent.Children.Where(n => !typeof(ShaderProgram).IsInstanceOfType(n)).ToList();


                processInvalidNodes(parent, invalid, parent.ToString(), out passed);
            }
        }

        #endregion

        /// <summary>
        /// Process any invalid nodes found, pruning out each from the Scene Graph as each is discovered.
        /// This transformation applies to immediate children only.
        /// A new validation state to the node is determined after the processing.
        /// </summary>
        private static void processInvalidNodes(SceneGraphNode parent, List<SceneGraphNode> invalid, string parentName, out bool passed, bool noChildrenAllowed = false)
        {
            string msg;

            passed = true;

            if (invalid.Any())
            {
                passed = false;

                msg = getNodeNames(invalid);

                if (noChildrenAllowed && debug) Console.WriteLine("{0} should not contain any children", parentName);
                if (debug) Console.WriteLine("{0} should not contain children of type [{1}]", parentName, msg);

                // Maybe it would be better to re-insert nodes in places where they are allowed instead of pruning them?
                //TODO: define node insertion rules
                pruneBadRelationships(parent, invalid);
            }
        }

        /// <summary>
        /// Removes nodes classed as invalid from the Scene Graph
        /// </summary>
        private static void pruneBadRelationships(SceneGraphNode parent, List<SceneGraphNode> invalidNodes)
        {
            for (int i = 0; i < invalidNodes.Count(); i++)
            {
                SceneGraphNode invalidNode = invalidNodes[i];

                //if(invalidNode.isValid.HasValue && invalidNode.isValid.Value == false)
                {
                    parent.Children.Remove(invalidNode);
                }
            }

            if (debug) Console.Write("pruned bad relationships ");
            if (debug) Console.WriteLine(parent.ErrorStringWithLineNumbers());
        }

        /// <summary>
        /// Removes a node from the Scene Graph that is classed as invalid 
        /// </summary>
        private static void pruneBadRelationship(SceneGraphNode invalidNode, bool referToParent = false)
        {
            //if (invalidNode.isValid.HasValue && invalidNode.isValid.Value == false)
            {
                if (referToParent)
                {
                    if (parent.Parent != null) parent.Parent.Children.Remove(invalidNode);
                }
                else
                {
                    parent.Children.Remove(invalidNode);
                }


            }

            if (debug) Console.Write("pruned bad relationship ");
            if (debug) Console.WriteLine(parent.ErrorStringWithLineNumbers());
        }

        /// <returns>
        /// List of node names in the set of input nodes
        /// </returns>
        private static string getNodeNames(IEnumerable<SceneGraphNode> nodes)
        {
            string msg = string.Empty;
            foreach (SceneGraphNode n in nodes)
            {
                msg += n.ToString() + " ";
            }
            msg = msg.TrimEnd().Replace(" ", ", ");

            return msg;
        }

    }
}
