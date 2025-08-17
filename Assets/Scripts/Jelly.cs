using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataStructures.UnionFind;

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


    public enum Direction
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
    }

    public class Grid
    {
        // Cells reference index of the block if it is a block, -1 for being empty and -2 for being obstacle.
        private int[] cells;
        private string[] cellStrings;
        private List<Block> blocks;
        private int columns;
        private int rows;
        private DisjointSet<Chunk> chunkSet;


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
            chunkSet = new DisjointSet<Chunk>();

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
                                Block newBlock = new Block(cellString);
                                blocks.Add(newBlock);
                                AddNewSetsForBlock(newBlock);
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

        public bool TryInsertBlock(int row, int column, Block block)
        {
            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return false;
            }
            if (block == null || block.GetChunkCount() == 0)
            {
                return false;
            }
            int cellIndex = column + row * columns;
            if (cells[cellIndex] != (int) CellMarker.Empty || cells[cellIndex] >= blocks.Count)
            {
                return false; // Cell is not empty or is an obstacle
            }
            cells[cellIndex] = blocks.Count;
            blocks.Add(block);
            AddNewSetsForBlock(block);
            ResolveAt(row, column);

            // Print debug information
            Chunk[][] chunkSetArray = chunkSet.GetAllSets();
            for (int i = 0; i < chunkSetArray.Length; i++)
            {
                Debug.Log($"Chunk Set {i}:");
                foreach (Chunk chunk in chunkSetArray[i])
                {
                    Debug.Log($"  Chunk Color: {chunk.ColorCode}");
                }
            }

            return true;
        }

        
        private void ResolveAt(int row, int column)
        {
            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return;
            }
            int cellIndex = column + row * columns;
            if (cells[cellIndex] < 0 || cells[cellIndex] >= blocks.Count)
            {
                return; // Cell is empty or an obstacle
            }
            Block block = blocks[cells[cellIndex]];
            
            // Check neighbour blocks in four directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new (0, -1), // Left
                new (0, 1),  // Right
                new (-1, 0),  // Up
                new (1, 0),  // Down
            };

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int dir = directions[i];
                int neighbourRow = row + dir.x;
                int neighbourColumn = column + dir.y;
                if (neighbourRow < 0 || neighbourRow >= rows || neighbourColumn < 0 || neighbourColumn >= columns)
                {
                    continue; // Out of bounds
                }
                int neighbourIndex = neighbourColumn + neighbourRow * columns;
                if (cells[neighbourIndex] < 0 || cells[neighbourIndex] >= blocks.Count)
                {
                    continue; // Cell is empty or an obstacle
                }
                Block neighbourBlock = blocks[cells[neighbourIndex]];
                
                switch (i)
                {
                    case (int) Direction.Left:
                        {
                            foreach (Chunk chunk in block.GetLeft())
                            {
                                foreach (Chunk neighbourChunk in neighbourBlock.GetRight())
                                {
                                    if (chunk.ColorCode == neighbourChunk.ColorCode)
                                    {
                                        chunkSet.Union(chunk, neighbourChunk);
                                    }
                                }
                            }
                            break;
                        }
                    case (int) Direction.Right:
                        {
                            foreach (Chunk chunk in block.GetRight())
                            {
                                foreach (Chunk neighbourChunk in neighbourBlock.GetLeft())
                                {
                                    if (chunk.ColorCode == neighbourChunk.ColorCode)
                                    {
                                        chunkSet.Union(chunk, neighbourChunk);
                                    }
                                }
                            }
                            break;
                        }
                    case (int) Direction.Up:
                        {
                            foreach (Chunk chunk in block.GetTop())
                            {
                                foreach (Chunk neighbourChunk in neighbourBlock.GetBottom())
                                {
                                    if (chunk.ColorCode == neighbourChunk.ColorCode)
                                    {
                                        chunkSet.Union(chunk, neighbourChunk);
                                    }
                                }
                            }
                            break;
                        }
                    case (int) Direction.Down:
                        {
                            Debug.Log($"Resolving Down at {row}, {column}");
                            foreach (Chunk chunk in block.GetBottom())
                            {
                                    foreach (Chunk neighbourChunk in neighbourBlock.GetTop())
                                    {
                                        Debug.Log($"Comparing {chunk.ColorCode} with {neighbourChunk.ColorCode}");
                                        if (chunk.ColorCode == neighbourChunk.ColorCode)
                                        {
                                            chunkSet.Union(chunk, neighbourChunk);
                                        }
                                    }
                            }
                            break;
                        }
                }
            }
        }

        private void AddNewSetsForBlock(Block block)
        {
            if (chunkSet == null || block == null || block.GetChunkCount() == 0)
            {
                return;
            }
            foreach (Chunk chunk in block.GetChunks())
            {
                chunkSet.MakeSet(chunk);
            }
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
                    new int[] { 1 }
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

        public List<Chunk> GetChunks()
        {
            return chunks;
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
    public class Chunk : IComparable<Chunk>
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

        // This is 99.9% unnecessary, due to Union-Find implementation didn't actually make use
        // of any comparision other than equal. But I'm not risking editing that code
        public int CompareTo(Chunk other)
        {
            if (other == null)
            {
                return 1;
            }
            if (this == other)
            {
                return 0;
            }
            return -1;
        }
    }
}