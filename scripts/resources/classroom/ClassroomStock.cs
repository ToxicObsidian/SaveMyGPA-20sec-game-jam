using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using ZLinq;

[GlobalClass]
public partial class ClassroomStock : Resource
{
    [Export]
    public Array<ClassroomStockItem> Items { get; protected set; }


    public ClassroomStockItem GetRandomOne(List<string> excluded_names)
    {
        var excluded_set = new HashSet<string>(excluded_names);

        var valid_set = Items
            .AsValueEnumerable()
            .Where(item => item != null && !excluded_set.Contains(item.ClassroomName))
            .ToList();

        if (valid_set.Count < 1)
        {
            return null;
        }

        return valid_set[new Random().Next(0, valid_set.Count)];
    }

    public ClassroomStockItem GetItemByName(string name)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].ClassroomName == name) return Items[i];
        }

        throw new KeyNotFoundException(name + " is invalid.");
    }
}
