using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Jelly
{
    public enum ColorCode
    {
        Red = 'r',
        Green = 'g',
        Blue = 'b',
        Yellow = 'y',
        Purple = 'p',
        Pink = 'k', 
    }

    public enum CellMarker
    {
        Obstacle = -2,
        Empty = -1,
    }
    
    public enum BlockVariant
    {
        Single,
        DoubleHorizontal,
        DoubleVertical,
        TripleLeft,
        TripleRight,
        TripleTop,
        TripleBottom,
    }


    public class Grid
    {
        // Cells reference index of the block if it is a block, -1 for being empty and -2 for being obstacle.
        private int[] cells;
        private string[] cellStrings;
        private List<Block> blocks;
        private int columns;
        private int rows;

        public int Columns { get { return columns; } }
        public int Rows { get { return rows; } }


        public Grid(TextAsset layoutFile)
        {
            char[] charDelimiters = new[] { '\n', '\r' };
            StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries;
            string[] lines = layoutFile.text.Split(charDelimiters, splitOptions);

            if (lines.Length == 0 || lines.Length % 2 != 0 || lines[0].Length == 0 || lines[0].Length % 2 != 0)
            {
                return;
            }

            rows = lines.Length / 2;
            columns = lines[0].Length / 2;
            
            cells = new int[columns * rows];
            blocks = new List<Block>();
            cellStrings = new string[columns * rows];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int top = row * 2;
                    int left = col * 2;

                    // Extract the 2x2 block of chars
                    string cellString =
                        lines[top].Substring(left, 2) +
                        lines[top + 1].Substring(left, 2);

                    int index = col + row * columns;
                    cellStrings[index] = cellString;

                    switch (cellString)
                    {
                        case "####":
                            {
                                cells[index] = (int)CellMarker.Obstacle;
                                break;
                            }
                        case "----":
                            { 
                                cells[index] = (int)CellMarker.Empty;
                                break;
                            }
                        default:
                            {
                                cells[index] = blocks.Count;
                                blocks.Add(new Block(cellString));
                                break;

                            }
                    }
                }
            }
        }


        public int GetCell(int row, int column)
        {
            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return (int) CellMarker.Obstacle;
            }
            return cells[column + row * columns];
        }

        public Block GetBlock(int row, int column)
        {
            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return null;
            }
            int cellIndex = cells[column + row * columns];
            if (cellIndex < 0 || cellIndex >= blocks.Count)
            {
                return null;
            }
            return blocks[cellIndex];
        }

        public string GetCellString(int row, int column)
        {
            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return "####"; // Obstacle
            }
            return cellStrings[column + row * columns];
        }
    }
    

    // A block of jelly, consisting of multiple colored chunks.
    public class Block
    {
        private BlockVariant variant;
        private List<Chunk> chunks;
        private List<Chunk> left;
        private List<Chunk> right;
        private List<Chunk> top;
        private List<Chunk> bottom;
        
        public BlockVariant Variant { get { return variant; } }


        public Block(string colorCharCodes)
        {
            if (colorCharCodes == null || colorCharCodes.Length != 4)
            {
                return;
            }

            chunks = new List<Chunk>();

            char c0 = colorCharCodes[0];
            char c1 = colorCharCodes[1];
            char c2 = colorCharCodes[2];
            char c3 = colorCharCodes[3];

            // One colored block. Ex:
            // 00
            // 00
            if (c0 == c1 && c1 == c2 && c2 == c3)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));

                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 0, 1 },
                    new int[] { 0, 1 },
                    new int[] { 0 },
                    new int[] { 0 }
                };

                SetAllByIndices(getDir);
                
                variant = BlockVariant.Single;
                return;
            }

            // Two colored block. 2 Variants:
            // 1. Horizontal: Ex:
            // Pos:    Color:
            // 01   -> 00
            // 23      11
            if (c0 == c1 && c2 == c3 && c0 != c2)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c2));

                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 0, 1 },
                    new int[] { 0, 1 },
                    new int[] { 0 },
                    new int[] { 0 }
                };
                SetAllByIndices(getDir);

                variant = BlockVariant.DoubleHorizontal;
                return;
            }
            // 2. Vertical: Ex:
            // Pos:    Color:
            // 01   -> 01
            // 23      01
            if (c0 == c2 && c1 == c3 && c0 != c1)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c1));
                
                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 0 },
                    new int[] { 1 },
                    new int[] { 0, 1 },
                    new int[] { 0, 1 }
                };
                SetAllByIndices(getDir);

                variant = BlockVariant.DoubleVertical;
                return;
            }

            // Three colored block. 4 Variants:
            // Pos:    Color:
            // 01   -> 01
            // 23      02
            if (c0 == c2 && c1 != c3 && c0 != c1 && c0 != c3)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c1));
                chunks.Add(new Chunk(this, (ColorCode) c3));

                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 0 },
                    new int[] { 1, 2 },
                    new int[] { 0, 1 },
                    new int[] { 0, 2 }
                };
                SetAllByIndices(getDir);

                variant = BlockVariant.TripleLeft;
                return;
            }
            // Pos:    Color:
            // 01   -> 00
            // 23      12
            if (c0 == c1 && c2 != c3 && c0 != c2 && c0 != c3)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c2));
                chunks.Add(new Chunk(this, (ColorCode) c3));

                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 0, 1 },
                    new int[] { 0, 2 },
                    new int[] { 0 },
                    new int[] { 1, 2 }
                };
                SetAllByIndices(getDir);

                variant = BlockVariant.TripleTop;
                return;
            }
            // Pos:    Color:
            // 01   -> 10
            // 23      20
            if (c1 == c3 && c0 != c2 && c0 != c1 && c1 != c2)
            {
                chunks.Add(new Chunk(this, (ColorCode) c1));
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c2));

                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 1, 2 },
                    new int[] { 0 },
                    new int[] { 1, 0 },
                    new int[] { 2, 0 }
                };
                SetAllByIndices(getDir);

                variant = BlockVariant.TripleRight;
                return;
            }
            // Pos:    Color:
            // 01   -> 12
            // 23      00
            if (c2 == c3 && c0 != c1 && c0 != c2 && c1 != c2)
            {
                chunks.Add(new Chunk(this, (ColorCode) c2));
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c1));

                // Left - Right - Top - Bottom
                int[][] getDir =
                {
                    new int[] { 1, 0 },
                    new int[] { 2, 0 },
                    new int[] { 1, 2 },
                    new int[] { 0 }
                };
                SetAllByIndices(getDir);

                variant = BlockVariant.TripleBottom;
                return;
            }

            // Support more if needed
        }


        public void RemoveChunk(Chunk chunk)
        {
            if (chunks.Contains(chunk))
            {
                chunks.Remove(chunk);
            }
        }
        public List<Chunk> GetLeft()
        {
            return left;
        }
        public List<Chunk> GetRight()
        {
            return right;
        }
        public List<Chunk> GetTop()
        {
            return top;
        }

        public List<Chunk> GetBottom()
        {
            return bottom;
        }

        public ColorCode[] GetChunkColorCodes()
        {
            ColorCode[] colorCodes = new ColorCode[chunks.Count];
            for (int i = 0; i < chunks.Count; i++)
            {
                colorCodes[i] = chunks[i].ColorCode;
            }
            return colorCodes;
        }

        public int GetChunkCount()
        {
            if (chunks == null)
            {
                return 0;
            }
            return chunks.Count;
        }

        public string GetColorCharCodes()
        {
            if (chunks == null || chunks.Count == 0)
            {
                return "????";
            }
            string colorCodes = "";
            foreach (Chunk chunk in chunks)
            {
                colorCodes += (char) chunk.ColorCode;
            }
            return colorCodes;
        }

        private void SetAllByIndices(int[][] getDir)
        {
            left = new List<Chunk>();
            right = new List<Chunk>();
            top = new List<Chunk>();
            bottom = new List<Chunk>();
            for (int i = 0; i < getDir[0].Length; i++)
            {
                if (getDir[0][i] >= 0 && getDir[0][i] < chunks.Count)
                {
                    left.Add(chunks[getDir[0][i]]);
                }
            }
            for (int i = 0; i < getDir[1].Length; i++)
            {
                if (getDir[1][i] >= 0 && getDir[1][i] < chunks.Count)
                {
                    right.Add(chunks[getDir[1][i]]);
                }
            }
            for (int i = 0; i < getDir[2].Length; i++)
            {
                if (getDir[2][i] >= 0 && getDir[2][i] < chunks.Count)
                {
                    top.Add(chunks[getDir[2][i]]);
                }
            }
            for (int i = 0; i < getDir[3].Length; i++)
            {
                if (getDir[3][i] >= 0 && getDir[3][i] < chunks.Count)
                {
                    bottom.Add(chunks[getDir[3][i]]);
                }
            }
        }
    }

    // A colored chunk of a jelly block
    public class Chunk : UnityEngine.Object
    {
        public ColorCode ColorCode { get { return colorCode; } }

        private Block parentBlock;
        private ColorCode colorCode;

        public Chunk(Block parentBlock, ColorCode colorCode)
        {
            this.parentBlock = parentBlock;
            this.colorCode = colorCode;
        }

        public void RemoveFromBlock()
        {
            if (parentBlock != null)
            {
                parentBlock.RemoveChunk(this);
                parentBlock = null;
            }
        }
    }
}