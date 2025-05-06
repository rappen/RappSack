using Microsoft.Xrm.Sdk;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rappen.XRM.RappSack
{
    public class ContextEntity
    {
        private IPluginExecutionContext5 context;
        private string preImageName;
        private string postImageName;
        private Entity target;
        private Entity pre;
        private Entity post;

        public int Index;

        /// <summary>
        /// Contructor of ContextEntity to access Target, PreImage, PostImage and Complete
        /// </summary>
        /// <param name="context">IPluginExecutionContext5 is all you need to use this class</param>
        /// <param name="preImageName">If you have a specific pre image name, enter it, otherwise to first one will be used</param>
        /// <param name="postImageName">If you have a specific post image name, enter it, otherwise to first one will be used</param>
        /// <param name="index">Index is only used for collection of targets, pre/post images</param>
        public ContextEntity(IPluginExecutionContext5 context, string preImageName = null, string postImageName = null, int index = -1)
        {
            this.context = context;
            this.preImageName = preImageName;
            this.postImageName = postImageName;
            Index = index;
        }

        /// <summary>
        /// Get the Entity by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Entity this[ContextEntityType type]
        {
            get
            {
                switch (type)
                {
                    case ContextEntityType.Target:
                        if (Index < 0)
                        {
                            if (target != null)
                            {
                                return target;
                            }
                            if (context.InputParameters.TryGetValue(ParameterName.Target, out Entity _target))
                            {
                                target = _target;
                            }
                            if (context.InputParameters.TryGetValue(ParameterName.Target, out EntityReference reference))
                            {
                                target = new Entity(reference.LogicalName, reference.Id);
                            }
                            return target;
                        }
                        else
                        {
                            if (context.InputParameters.TryGetValue(ParameterName.Targets, out EntityCollection targets) &&
                                targets.Entities.Count > Index)
                            {
                                return targets.Entities[Index];
                            }
                        }
                        break;

                    case ContextEntityType.PreImage:
                        if (Index < 0)
                        {
                            if (pre != null)
                            {
                                return pre;
                            }
                            if (context.PreEntityImages.Count > 0)
                            {
                                pre = context.PreEntityImages.FirstOrDefault().Value;
                                return pre;
                            }
                        }
                        else
                        {
                            if (context.InputParameters.TryGetValue(ParameterName.Targets, out EntityCollection entities) &&
                                context.PreEntityImagesCollection?.Length == entities.Entities.Count() &&
                                context.PreEntityImagesCollection.Length > Index)
                            {
                                return context.PreEntityImagesCollection[Index]
                                    .FirstOrDefault(pre => string.IsNullOrEmpty(preImageName) ? !pre.Key.Equals("PreBusinessEntity") : pre.Key.Equals(preImageName)).Value;
                            }
                        }
                        break;

                    case ContextEntityType.PostImage:
                        if (Index < 0)
                        {
                            if (post != null)
                            {
                                return post;
                            }
                            if (context.PostEntityImages.Count > 0)
                            {
                                post = context.PostEntityImages.FirstOrDefault().Value;
                                return post;
                            }
                        }
                        else
                        {
                            if (context.InputParameters.TryGetValue(ParameterName.Targets, out EntityCollection entities) &&
                                context.PostEntityImagesCollection?.Length == entities.Entities.Count() &&
                                context.PostEntityImagesCollection.Length > Index)
                            {
                                return context.PostEntityImagesCollection[Index]
                                    .FirstOrDefault(post => string.IsNullOrEmpty(postImageName) ? !post.Key.Equals("PostBusinessEntity") : post.Key.Equals(postImageName)).Value;
                            }
                        }
                        break;

                    case ContextEntityType.Complete:
                        return this[ContextEntityType.Target].Merge(this[ContextEntityType.PostImage]).Merge(this[ContextEntityType.PreImage]);
                }
                return null;
            }
        }
    }

    public class ContextEntityCollection : IEnumerable<ContextEntity>
    {
        private List<ContextEntity> _entities = new List<ContextEntity>();

        /// <summary>
        /// Contructor of ContextEntityCollection to access a list of Targets, PreImages, PostImages and Complete
        /// </summary>
        /// <param name="context">IPluginExecutionContext5 is all you need to use this class</param>
        /// <param name="preImageName">If you have a specific pre image name, enter it, otherwise to first one will be used</param>
        /// <param name="postImageName">If you have a specific post image name, enter it, otherwise to first one will be used</param>
        public ContextEntityCollection(IPluginExecutionContext5 context, string preImageName = null, string postImageName = null)
        {
            if (context?.InputParameters?.Contains(ParameterName.Targets) == true &&
                context.InputParameters[ParameterName.Targets] is EntityCollection entityCollection)
            {
                var i = 0;
                entityCollection.Entities.ToList().ForEach(_ => _entities.Add(new ContextEntity(context, preImageName, postImageName, i++)));
            }
        }

        public IEnumerator<ContextEntity> GetEnumerator() => _entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public enum ContextEntityType
    {
        Target,
        PreImage,
        PostImage,
        Complete
    }
}