using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHidePropertyAttribute : PropertyAttribute
{
    public string ConditionalSourceField = "";
    public bool HideInInspector = false;
    public bool BoolValue = true;

    public ConditionalHidePropertyAttribute(string conditionalSourceField)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.HideInInspector = false;
    }

    public ConditionalHidePropertyAttribute(string conditionalSourceField, bool BoolValue)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.HideInInspector = false;
        this.BoolValue = BoolValue;
    }

}
