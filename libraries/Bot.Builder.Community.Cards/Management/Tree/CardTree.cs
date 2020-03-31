using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Cards.Management.Tree
{
    internal static class CardTree
    {
        private const string SpecifyManually = " Try specifying the node type manually instead of using null.";

        /// <summary>
        /// Enters and exits the tree at the specified nodes.
        /// </summary>
        /// <typeparam name="TEntry">The .NET type of the entry node.</typeparam>
        /// <typeparam name="TExit">The .NET type of the exit node.</typeparam>
        /// <param name="entryValue">The entry value.</param>
        /// <param name="action">A delegate to perform on each exit value.
        /// Note that exit values are guaranteed to be non-null.</param>
        /// <param name="entryNode">The explicit position of the entry node in the tree.
        /// If this is null then the position is inferred from the TEntry type parameter.
        /// Note that this parameter is required if the type is <see cref="object"/>
        /// or if the position otherwise cannot be unambiguously inferred from the type.</param>
        /// <param name="exitNode">The explicit position of the exit node in the tree.
        /// If this is null then the position is inferred from the TExit type parameter.
        /// Note that this parameter is required if the type is <see cref="object"/>
        /// or if the position otherwise cannot be unambiguously inferred from the type.</param>
        /// <param name="modifiesChildren">True if each child should be reassigned to its parent during recursion
        /// (which breaks Adaptive Card attachment content references when they get converted to a
        /// <see cref="JObject"/> and back), false if each original reference should remain.</param>
        /// <returns>The possibly-modified entry value. This is needed if a new object was created
        /// to modify the value, such as when an Adaptive Card is converted to a <see cref="JObject"/>.</returns>
        internal static TEntry Recurse<TEntry, TExit>(
                TEntry entryValue,
                Action<TExit> action,
                ITreeNode entryNode = null,
                ITreeNode exitNode = null,
                bool modifiesChildren = false)
            where TEntry : class
            where TExit : class
        {
            try
            {
                entryNode = GetNode<TEntry>(entryNode);
            }
            catch (Exception ex)
            {
                throw GetNodeArgumentException<TEntry>(ex);
            }

            try
            {
                exitNode = GetNode<TExit>(exitNode);
            }
            catch (Exception ex)
            {
                throw GetNodeArgumentException<TExit>(ex, "exit");
            }

            Task<object> Next(object childValue, ITreeNode childNode)
            {
                if (childNode == exitNode)
                {
                    if (GetExitValue<TExit>(childValue) is TExit typedChild)
                    {
                        action(typedChild);
                    }
                }
                else
                {
                    // CallChildAsync will be executed immediately even though it's not awaited
                    var task = childNode.CallChildAsync(childValue, Next);

                    if (modifiesChildren)
                    {
                        return task;
                    }
                }

                return Task.FromResult(childValue);
            }

            return entryNode.CallChildAsync(entryValue, Next).Result as TEntry;
        }

        internal static TEntry ApplyIds<TEntry>(TEntry entryValue, PayloadIdOptions options = null, ITreeNode entryNode = null)
            where TEntry : class
        {

            //Recurse(
            //    entryValue,
            //    (object payload) =>
            //    {
            //        payload.ToJObject(true).ApplyIdsToPayload(options);
            //    },
            //    entryType,
            //    TreeNodeType.Payload,
            //    true);


            try
            {
                entryNode = GetNode<TEntry>(entryNode);
            }
            catch (Exception ex)
            {
                throw GetNodeArgumentException<TEntry>(ex);
            }

            void ProcessOptions(ITreeNode node)
            {
                if (node.IdType is string idType)
                {
                    options = (options ?? new PayloadIdOptions()).ReplaceNullWithGeneratedId(idType);
                }
            }

            Task<object> Next(object childValue, ITreeNode childNode)
            {
                if (childNode == TreeNodes.Payload)
                {
                    // This local function is not async
                    // so just return the task without awaiting it
                    return childValue.ToJObjectAndBackAsync(
                        payload =>
                        {
                            payload.ApplyIdsToPayload(options);

                            return Task.CompletedTask;
                        }, true);
                }
                else
                {
                    if (childNode == TreeNodes.SubmitAction)
                    {
                        // We need to create a "data" object in the submit action
                        // if there isn't one already
                        childValue = childValue.ToJObjectAndBackAsync(submitAction =>
                        {
                            if (submitAction.GetValue(CardConstants.KeyData).IsNullish())
                            {
                                submitAction.SetValue(CardConstants.KeyData, new JObject());
                            }

                            return Task.CompletedTask;
                        }).Result;
                    }

                    var capturedOptions = options;

                    ProcessOptions(childNode);

                    // CallChildAsync will be executed immediately even though it's not awaited
                    var task = childNode.CallChildAsync(childValue, Next);

                    options = capturedOptions;

                    return task;
                }
            }

            ProcessOptions(entryNode);

            return entryNode.CallChildAsync(entryValue, Next).Result as TEntry;
        }

        internal static ISet<PayloadItem> GetIds<TEntry>(TEntry entryValue, ITreeNode entryNode = null)
            where TEntry : class
        {
            var ids = new HashSet<PayloadItem>();

            Recurse(
                entryValue,
                (PayloadItem payloadId) =>
                {
                    ids.Add(payloadId);
                }, entryNode);

            return ids;
        }

        private static TExit GetExitValue<TExit>(object child)
            where TExit : class => child is JToken jToken && !typeof(JToken).IsAssignableFrom(typeof(TExit)) ? jToken.ToObject<TExit>() : child as TExit;

        private static ITreeNode GetNode<T>(ITreeNode node)
        {
            var t = typeof(T);

            if (node is null)
            {
                if (t == typeof(object))
                {
                    throw new Exception("A node cannot be automatically determined from a System.Object type argument." + SpecifyManually);
                }

                var matchingNodes = new List<ITreeNode>();

                foreach (var possibleNode in TreeNodes.Collection)
                {
                    var possibleNodeTValue = possibleNode.GetTValue();

                    if (possibleNodeTValue.IsAssignableFrom(t) && possibleNodeTValue != typeof(object) && possibleNodeTValue != typeof(IEnumerable<object>))
                    {
                        matchingNodes.Add(possibleNode);
                    }
                }

                var count = matchingNodes.Count();

                if (count < 1)
                {
                    throw new Exception($"No node exists that's assignable from the type argument: {t}. Try using a different type.");
                }

                if (count > 1)
                {
                    throw new Exception($"Multiple nodes exist that are assignable from the type argument: {t}." + SpecifyManually);
                }

                return matchingNodes.First();
            }

            return node.GetTValue().IsAssignableFrom(t)
                ? node
                : throw new Exception($"The node type {node} is not assignable from the type argument: {t}."
                    + " Make sure you're providing the correct node type.");
        }

        private static ArgumentException GetNodeArgumentException<TEntry>(Exception inner, string entryOrExit = "entry")
        {
            return new ArgumentException(
                $"The {entryOrExit} node could not be determined from the type argument: {typeof(TEntry)}.",
                $"{entryOrExit}Type",
                inner);
        }
    }
}
