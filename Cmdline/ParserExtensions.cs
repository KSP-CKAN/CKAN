using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace CKAN.CmdLine
{
    /// <summary>
    /// Attribute to let the help screen recognize what the nested verbs are.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ChildVerbsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CKAN.CmdLine.ChildVerbsAttribute"/> class.
        /// </summary>
        /// <param name="types">A <see cref="System.Type"/> array used to supply nested verb alternatives.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="types"/> array is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="types"/> array is empty.</exception>
        public ChildVerbsAttribute(params Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            if (types.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(types));

            Types = types;
        }

        /// <summary>
        /// Gets the types of the nested verbs.
        /// </summary>
        public Type[] Types { get; }
    }

    /// <summary>
    /// Attribute to exclude the nested verbs from the main help screen.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class VerbExclude : Attribute { }

    /// <summary>
    /// Extension methods to allow multi level verb parsing.
    /// </summary>
    public static class ParserVerbExtensions
    {
        /// <summary>
        /// Parses a string array of arguments into the command line.
        /// </summary>
        /// <param name="parser">A <see cref="CommandLine.Parser"/> instance.</param>
        /// <param name="args">A <see cref="System.String"/> array of command line arguments, to parse into the command line.</param>
        /// <param name="types">A <see cref="System.Type"/> array used to supply verb alternatives.</param>
        /// <returns>A <see cref="CommandLine.ParserResult{T}"/> containing the appropriate instance with parsed values as a <see cref="System.Object"/> and a sequence of <see cref="CommandLine.Error"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="parser"/>, one or more arguments, or if the <paramref name="types"/> array is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="types"/> array is empty.</exception>
        public static ParserResult<object> ParseVerbs(this Parser parser, IEnumerable<string> args, params Type[] types)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (types == null)
                throw new ArgumentNullException(nameof(types));

            if (types.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(types));

            var argsArray = args as string[] ?? args.ToArray();
            if (argsArray.Length == 0 || argsArray[0].StartsWith("-"))
                return parser.ParseArguments(argsArray, types);

            var verb = argsArray[0];
            foreach (var type in types)
            {
                var verbAttribute = type.GetCustomAttribute<VerbAttribute>();
                if (verbAttribute == null || verbAttribute.Name != verb)
                    continue;

                var subVerbsAttribute = type.GetCustomAttribute<ChildVerbsAttribute>();
                if (subVerbsAttribute != null)
                    return ParseVerbs(parser, argsArray.Skip(1).ToArray(), subVerbsAttribute.Types);

                break;
            }

            return parser.ParseArguments(argsArray, types);
        }
    }
}
