using System;
using System.Collections.Generic;

namespace IeltsTeachingAssistant.Models;

public static class SelectedRubricDescriptorsExtensions
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, string> _descriptors = new();

    public static string GetSelectedRubricDescriptors(this object obj)
    {
        return _descriptors.TryGetValue(obj, out var val) ? val : string.Empty;
    }

    public static void SetSelectedRubricDescriptors(this object obj, string val)
    {
        _descriptors.Remove(obj);
        _descriptors.Add(obj, val);
    }
}
