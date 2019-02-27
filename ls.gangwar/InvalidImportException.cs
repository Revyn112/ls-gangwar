using System;
using AltV.Net;
using AltV.Net.Native;

namespace ls.gangwar
{
    public class InvalidImportException : Exception
    {
        public static Exception Create(string resourceName, string key, MValue.Type type)
        {
            if (!Alt.Import(resourceName, key, out MValue mValue))
            {
                return new InvalidImportException(
                    $"Resource: '{resourceName}' doesn't contains a export with the name: '{key}'.");
            }

            if (mValue.type != type)
            {
                return new InvalidImportException(
                    $"Resource: '{resourceName}.{key}' with type: '{mValue.type}' doesn't matches the expected export type: '{type}'.");
            }

            return new InvalidImportException(
                $"Resource: '{resourceName}' with export: '{key}' caused a unknown error.");
        }

        private InvalidImportException(string message)
            : base(message)
        {
        }
    }
}