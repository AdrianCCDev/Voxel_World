using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{

    public string name;

    public bool isSolid;
    public bool isTransparent;

    public Vector2 texture;

    public Vector2 textureUp;
    public Vector2 textureSide;
    public Vector2 textureBot;

    public bool multitexture;

    public bool isBillboard;

    public bool isBreakable;


    //Manage the different types of blocks that we can spawn on the world

    public Block(string _name, bool _solid, bool _transparent, Vector2 _texture, bool _isBillboard, bool _isBreakable)
    {
        name = _name;
        isSolid = _solid;
        isTransparent = _transparent;
        texture = _texture;
        isBillboard = _isBillboard;
        isBreakable = _isBreakable;

    }

    public Block(string _name, bool _solid, bool _transparent, bool _isBreakable, Vector2 _textureUp, Vector2 _textureSide, Vector2 _textureBot)
    {
        name = _name;
        isSolid = _solid;
        isTransparent = _transparent;
        isBreakable = _isBreakable;
        textureUp = _textureUp;
        textureSide = _textureSide;
        textureBot = _textureBot;

        multitexture = true;

    }

    public Block()
    {
        name = "Air";
        isSolid = false;
        isTransparent = true;
    }



}
