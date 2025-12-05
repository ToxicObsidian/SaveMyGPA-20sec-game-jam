#if false
using Godot;
using Godot.Collections;
using System.Collections.Generic;


[GlobalClass]
[Tool]
public partial class AnswerOptionsLayout: Control
{
    [ExportGroup("Answer Settings")]
    [Export]
    public Array<Texture2D> Answers 
    { 
        get { return _answers; }
        set
        {
            _answers = value;
            _is_data_changed = true;
        }
    }
    protected Array<Texture2D> _answers = new();

    [Export]
    public int Seperation
    {
        get { return _seperation; }
        set
        {
            _seperation = value;
            _is_data_changed = true;
        }
    }
    protected int _seperation = 0;


    [ExportGroup("Alignment")]
    [Export]
    public BoxContainer.AlignmentMode ContainerAlign
    {
        get { return _c_align; }
        set
        {
            _c_align = value;
            _is_data_changed = true;
        }
    }
    protected BoxContainer.AlignmentMode _c_align = BoxContainer.AlignmentMode.Begin;
    [Export]
    public ContainerType CurrentContainerType
    {
        get { return _current_container_type; }
        set
        {
            _current_container_type = value;
            _UpdateContainerType();
            _is_data_changed = true;
        }
    }
    protected ContainerType _current_container_type = ContainerType.HBox;


    [ExportGroup("Texture Settings")]
    [Export]
    public TextureRect.ExpandModeEnum ExpandMode
    {
        get { return _texture_expand_mode; }
        set
        {
            _texture_expand_mode = value;
            _is_data_changed = true;
        }
    }
    protected TextureRect.ExpandModeEnum _texture_expand_mode = TextureRect.ExpandModeEnum.KeepSize;
    [Export]
    public Vector2 TextureScale
    {
        get { return _texture_scale; }
        set
        {
            _texture_scale = value;
            _is_data_changed = true;
        }
    }
    protected Vector2 _texture_scale = Vector2.One;

    protected BoxContainer _container;
    protected HBoxContainer _h_container = new();
    protected VBoxContainer _v_container = new();
    protected bool _is_data_changed = false;


    public enum ContainerType
    {
        HBox = 0,
        VBox = 1,
    }


    public override void _Ready()
    {
        _UpdateContainerType();
        _container.Name = "Answer Option Container";
        _container.SetAnchorsPreset(LayoutPreset.FullRect);
    }


    public override void _Process(double delta)
    {
        if (_is_data_changed)
        {
            _is_data_changed = false;
            _RebuildLayout();
        }
    }

    protected void _RebuildLayout()
    {
        if (!IsInsideTree()) return;

        QueueRedraw();
    }

    protected void _UpdateContainerType()
    {
        // 1. Unload rects in the current container.
        Array<Node> nodes;
        if (_container != null)
        {
            nodes = _container.GetChildren();
            RemoveChild(_container);
        }
        else nodes = new Array<Node>();

        foreach (Node rect in nodes)
        {
            _container.RemoveChild(rect);
        }

        // 2. Set the container to the current specified one.
        switch(_current_container_type)
        {
            case ContainerType.HBox:
                _container = _h_container;
                break;
            case ContainerType.VBox:
                _container = _v_container;
                break;
        }

        // 3. Move the rects to the updated one.
        foreach (Node rect in nodes)
        {
            _container.AddChild(rect);
        }
        AddChild(_container);

        // 4. Update the container related attributes.
        _UpdateContainerSettings();
    }

    protected void _UpdateContainerSettings()
    {
        _container.Alignment = _c_align;
        _container.AddThemeConstantOverride("seperation", Seperation);
    }

    protected void _UpdateTextureSettings()
    {
        foreach (Node node in _container.GetChildren())
        {
            if (node is not TextureRect rect) continue;
            else
            {
                rect.ExpandMode = _texture_expand_mode;
                if (_texture_scale != Vector2.One)
                {
                    rect.CustomMinimumSize = rect.Texture.GetSize() * _texture_scale;
                }
            }
        }
    }
}
#endif