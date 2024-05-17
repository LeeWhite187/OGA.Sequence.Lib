using OGA.Sequence.Model.Transitions;
using OGA.Sequence.Model.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OGA.Sequence.Factory
{
    /// <summary>
    /// Provides a central clearinghouse for generating step instances of a given type.
    /// Once initialized, can generate steps by name, from all referenced assemblies.
    /// </summary>
    public class SequenceFactory
    {
        static private Dictionary<string, SequenceFactory_abstract> faclist;
        static private Dictionary<string, string> steptypecachelist;
        static private Dictionary<string, string> transitiontypecachelist;


        /// <summary>
        /// Public static call to create a task step by type name.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        static public (int res, TaskStep_abstract instance) GetStepInstance(string typename)
#else
        static public (int res, TaskStep_abstract? instance) GetStepInstance(string typename)
#endif
        {
            if (string.IsNullOrEmpty(typename))
                return (-1, null);

            var searchterm = typename.ToLower();

            TaskStep_abstract obj = null;

            // Attempt to get the step factory from the cache...
            var sfc = GetStepFactoryfromCache(searchterm);
            if(sfc != null)
            {
                // Cache hit.

                // Create the instance...
                obj = sfc.CreateStepbyName(searchterm);
                if(obj == null)
                {
                    // Failed to create instance.
                    // Cache is bad.

                    // Remove the cache entry...
                    steptypecachelist.Remove(searchterm);
                }
                else
                {
                    // Found it.
                    // Return it...
                    return (1, obj);
                }
            }
            // If here, we must iterate factories.

            // Iterate the factories until one gives us an instance...
            foreach(var sf in faclist)
            {
                // See if the current factory gives us our step type...
                obj = sf.Value.CreateStepbyName(searchterm);
                if(obj == null)
                {
                    // Failed to create instance.
                    // Not in current factory.
                    continue;
                }
                // If here, the current factory generated our step type.
                // We will cache it, and return.

                // Cache an entry of what factory can make our step...
                steptypecachelist.Add(searchterm, sf.Key);

                // Return the instance...
                return (1, obj);
            }
            // If here, we exhausted the factory listing, without a hit.

            // Return not found...
            return (-1, null);
        }

        /// <summary>
        /// Public static call to create a task transition by type name.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        static public (int res, Transition_abstract instance) GetTransitionInstance(string typename)
#else
        static public (int res, Transition_abstract? instance) GetTransitionInstance(string typename)
#endif
        {
            if (string.IsNullOrEmpty(typename))
                return (-1, null);

            var searchterm = typename.ToLower();

            Transition_abstract obj = null;

            // Attempt to get the transition factory from the cache...
            var sfc = GetTransitionFactoryfromCache(searchterm);
            if(sfc != null)
            {
                // Cache hit.

                // Create the instance...
                obj = sfc.CreateTransitionbyName(searchterm);
                if(obj == null)
                {
                    // Failed to create instance.
                    // Cache is bad.

                    // Remove the cache entry...
                    transitiontypecachelist.Remove(searchterm);
                }
                else
                {
                    // Found it.
                    // Return it...
                    return (1, obj);
                }
            }
            // If here, we must iterate factories.

            // Iterate the factories until one gives us an instance...
            foreach(var sf in faclist)
            {
                // See if the current factory gives us our transition type...
                obj = sf.Value.CreateTransitionbyName(searchterm);
                if(obj == null)
                {
                    // Failed to create instance.
                    // Not in current factory.
                    continue;
                }
                // If here, the current factory generated our transition type.
                // We will cache it, and return.

                // Cache an entry of what factory can make our transition...
                transitiontypecachelist.Add(searchterm, sf.Key);

                // Return the instance...
                return (1, obj);
            }
            // If here, we exhausted the factory listing, without a hit.

            // Return not found...
            return (-1, null);
        }

        /// <summary>
        /// Checks the cache to see if a factory has been defined for the given step type.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        static private SequenceFactory_abstract GetStepFactoryfromCache(string typename)
#else
        static private SequenceFactory_abstract? GetStepFactoryfromCache(string typename)
#endif
        {
            SequenceFactory_abstract sf = null;
            try
            {
                // Lookup the step factory ID for the given step type...
                string res = steptypecachelist[typename] ?? "";
                if (string.IsNullOrEmpty(res))
                {
                    // Not found.
                    return null;
                }
                // We got a cache hit.

                sf = faclist[res];
                return sf;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// Checks the cache to see if a factory has been defined for the given transition type.
        /// Returns 1 if found, other not found.
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
#if (NET452 || NET48)
        static private SequenceFactory_abstract GetTransitionFactoryfromCache(string typename)
#else
        static private SequenceFactory_abstract? GetTransitionFactoryfromCache(string typename)
#endif
        {
            SequenceFactory_abstract sf = null;
            try
            {
                // Lookup the step factory ID for the given transition type...
                string res = transitiontypecachelist[typename] ?? "";
                if (string.IsNullOrEmpty(res))
                {
                    // Not found.
                    return null;
                }
                // We got a cache hit.

                sf = faclist[res];
                return sf;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Call this method once on process startup.
        /// It will search referenced assemblies for sequence factory classes to generate objects by name.
        /// </summary>
        /// <returns></returns>
        static public int Initialize()
        {
            // Initialize our factory listing...
            faclist = new Dictionary<string, SequenceFactory_abstract>();

            // Initialize the cached step listing...
            steptypecachelist = new Dictionary<string, string>();
            transitiontypecachelist = new Dictionary<string, string>();

            // Add our local factory...
            // We will include a random key, so we can speed up later searches.
            var localfac = new SequenceFactory_LocalAssembly();
            faclist.Add(Guid.NewGuid().ToString(), localfac);


            // Add logic to iterate referenced assemblies for class types that are decorated with a factory attribute and contain the right calls...

            return 1;
        }
    }
}
