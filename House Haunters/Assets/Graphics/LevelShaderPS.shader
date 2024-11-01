Shader "Unlit/LevelShaderPS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //o.uv = v.uv;
                return o;
            }

            // this is tranferred from LevelShader.compute because webGL does not support compute shaders
            // NONE: 0
            #define AREA_VISUAL 1
            #define HIGHLIGHT 2
            #define OPTION 3
            #define HOVERED 4
            #define SELECTED 5

            #define PI 3.14159

            struct TileInfo {
                int floorType; // 0: ground, 1: wall, 2: pit
                int highlightType;
                int terrainController;
                int capturer; // 0: not a capture point, -1: no team, 1: left team, 2: right team, 3: both teams
            };

            float4 team1Color;
            float4 team2Color;

            //int pixPerTile;
            int tilesWide;
            int tilesTall;
            float t;

            int cursorTileX;
            int cursorTileY;
            int hoveredZoneCenterX;
            int hoveredZoneCenterY;
            //StructuredBuffer<TileInfo> _TileData;
            //Buffer<int> floorTypes;
            //Buffer<int> highlightTypes;
            //Buffer<int> terrainControllers;
            //Buffer<int> capturers;

            //uniform float floorTypes[361]; // 19x19 level
            //uniform float highlightTypes[361];
            //uniform float terrainControllers[361];
            //uniform float capturers[361];

            uniform float4 tileData[361]; // 19x19 level

            TileInfo getTile(int x, int y) {
                if (x < 0 || y < 0 || x >= tilesWide || y >= tilesTall) {
                    TileInfo junk;
                    junk.floorType = 0;
                    junk.terrainController = -1;
                    junk.highlightType = -1;
                    junk.capturer = 0;
                    return junk;
                }

                int index = x + tilesWide * y;
                float4 data = tileData[index];
                TileInfo tile;
                tile.floorType = data.x;
                tile.highlightType = data.y;
                tile.terrainController = data.z;
                tile.capturer = data.w;
                return tile;
            }

            float getDistFromEdge(float2 tileUV, bool checkLeft, bool checkRight, bool checkDown, bool checkUp) {
                float distance = 1;
                if (checkLeft) {
                    distance = min(distance, tileUV.x);
                }
                if (checkRight) {
                    distance = min(distance, 1 - tileUV.x);
                }
                if (checkDown) {
                    distance = min(distance, tileUV.y);
                }
                if (checkUp) {
                    distance = min(distance, 1 - tileUV.y);
                }
                return distance;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int2 tileDims = int2(tilesWide, tilesTall);
                int2 cursorTile = int2(cursorTileX, cursorTileY);
                int2 hoveredZoneCenter = int2(hoveredZoneCenterX, hoveredZoneCenterY);

                float2 stretchedUV = i.uv * tileDims;
                int2 tile = int2(stretchedUV);
                float2 tileUV = stretchedUV % 1;
                TileInfo tileData = getTile(tile.x, tile.y);

                fixed4 color = float4(0, 0, 0, 0);

                // fill in highlight
                switch (tileData.highlightType) {
                case HIGHLIGHT:
                    color = float4(0.9, 0.9, 0.7, 0.5);
                    break;
                case OPTION:
                    color = float4(0, 0.8, 0.8, 0.5 + 0.1 * sin(t * 4 * PI));
                    break;
                case HOVERED:
                    color = float4(0, 0.2, 0.8, 0.5 + 0.2 * sin(t * 8 * PI));
                    break;
                case SELECTED:
                    color = float4(0.5, 0.9, 0.5, 0.5 + 0.2 * sin(t * 2 * PI));
                    break;
                case AREA_VISUAL:
                    color = float4(0.9, 0.9, 0.7, 0.1);
                    break;
                }

                TileInfo leftTile = getTile(tile.x - 1, tile.y);
                TileInfo rightTile = getTile(tile.x + 1, tile.y);
                TileInfo aboveTile = getTile(tile.x, tile.y + 1);
                TileInfo belowTile = getTile(tile.x, tile.y - 1);

                // apply outline
                int2 zoneCenterDiff = hoveredZoneCenter - tile;
                int zoneSize = 2;
                if(abs(zoneCenterDiff.x) <= zoneSize && abs(zoneCenterDiff.y) <= zoneSize
                    && (abs(zoneCenterDiff.x) == zoneSize || abs(zoneCenterDiff.y) == zoneSize)
                ) {
                    // outline hovered capture zone
                    float lineThickness = 0.6;
                    float distFromEdge = getDistFromEdge(tileUV,
                        zoneCenterDiff.x == zoneSize,
                        zoneCenterDiff.x == -zoneSize,
                        zoneCenterDiff.y == zoneSize,
                        zoneCenterDiff.y == -zoneSize
                    );

                    if(distFromEdge <= lineThickness) {
                        float4 outlineColor = float4(0.9, 0.8, 0.1, 1);
                        float root = (1 - distFromEdge / lineThickness);
                        outlineColor.a = (0.7 + 0.2 * sin(8 * PI * t)) * root * root;

                        if(color.a > 0) {
                            outlineColor = outlineColor.a * float4(outlineColor.x, outlineColor.y, outlineColor.z, 0);
                            color += outlineColor;
                        } else {
                            color = outlineColor;
                        }
                    }
                }
                else if(tileData.terrainController != 0) {
                    // terrain controller
                    float distFromEdge = getDistFromEdge(tileUV, 
                        leftTile.terrainController != tileData.terrainController,
                        rightTile.terrainController != tileData.terrainController,
                        belowTile.terrainController != tileData.terrainController,
                        aboveTile.terrainController != tileData.terrainController
                    );
                    if(distFromEdge <= 0.04) {
                        color = tileData.terrainController == 1 ? team1Color : team2Color;
                    }
                }
                else if(tileData.capturer != 0) {
                    // capture point controller
                    float lineThickness = 0.2;
                    float distFromEdge = getDistFromEdge(tileUV,
                        leftTile.capturer != tileData.capturer,
                        rightTile.capturer != tileData.capturer,
                        belowTile.capturer != tileData.capturer,
                        aboveTile.capturer != tileData.capturer
                    );
                    float4 outlineColor = float4(0, 0, 0, 0);
                    if(distFromEdge <= lineThickness) {
                        switch (tileData.capturer) {
                        case -1:
                            outlineColor = float4(1, 1, 1, 1);
                            break;
                        case 1:
                            outlineColor = team1Color;
                            break;
                        case 2:
                            outlineColor = team2Color;
                            break;
                        case 3:
                            outlineColor = (team1Color + team2Color) / 2;
                            break;
                        }

                        float root = (1 - distFromEdge / lineThickness);
                        outlineColor.a = 0.3 * root * root;

                        if(color.a > 0) {
                            outlineColor = outlineColor.a * float4(outlineColor.x, outlineColor.y, outlineColor.z, 0);
                            color += outlineColor;
                        } else {
                            color = outlineColor;
                        }
                    }
                }

                if(tileData.floorType == 0) {
                    // add grid lines
                    bool isCursor = tile.x == cursorTile.x && tile.y == cursorTile.y;
                    float lineThickness = isCursor ? 0.05 : 0.015;
                    float outlineValue = isCursor ? 0.7 : 0.1;
                    if(tileUV.x < lineThickness || tileUV.x > 1 - lineThickness || tileUV.y < lineThickness || tileUV.y > 1 - lineThickness) {
                        float edgeSpot = tileUV.x < lineThickness || tileUV.x > 1 - lineThickness ? tileUV.y : tileUV.x; // 0 - 1 along the edge
                        int edgeSegments = 7;
                        int edgeIndex = edgeSpot * edgeSegments;
                        if(edgeIndex % 2 == 0) {
                            color = color.a > 0 ? color + float4(outlineValue, outlineValue, outlineValue, 0) : float4(1, 1, 1, outlineValue);
                        }
                    }
                }

                return color;
            }
            ENDCG
        }
    }
}
