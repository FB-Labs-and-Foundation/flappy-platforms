using System;

public class AttributeSettingsTitle : Attribute
{
	public AttributeSettingsTitle(string title)
	{
		this.title = title;
	}

	public string title { get; }
}