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
        None,
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
            
            return true;
        }

        public void Resolve()
        {
            Debug.Log("Initial state of grid:");
            for (int row = 0; row < rows; row++)
            {
                string rowString = "";
                for (int column = 0; column < columns; column++)
                {
                    int cellIndex = column + row * columns;
                    rowString += $"{cells[cellIndex]} ";
                }
                Debug.Log(rowString);
            }

            UnionAllChunks();
            
            Block[] involvedBlocks = RemoveNonDistinctChunks();
            Block[] blockArr = blocks.ToArray();

            // Remove blocks that have no chunks left
            List<Block> blocksToRemove = new List<Block>();
            List<int> shiftingHistory = new List<int>();

            foreach (Block block in involvedBlocks)
            {
                int blockIndex = blocks.IndexOf(block);
                
                if (blockIndex < 0 || blockIndex >= blocks.Count)
                {
                    continue;
                }
                if (block.Variant == BlockVariant.None)
                {
                    // Set the cell referenced this block to Empty
                    for (int i = 0; i < cells.Length; i++)
                    {
                        if (cells[i] == blockIndex)
                        {
                            cells[i] = (int)CellMarker.Empty;
                            break;
                        }
                    }
                    blockArr[blockIndex] = null;
                    blocksToRemove.Add(block);
                }
            }

            Debug.Log($"Initial blocks count: {blocks.Count}");
            Debug.Log($"Blocks to remove: {blocksToRemove.Count}");

            blocks = new List<Block>();
            for (int i = 0; i < blockArr.Length; i++)
            {
                if (blockArr[i] != null)
                {
                    blocks.Add(blockArr[i]);
                    for (int j = 0; j < cells.Length; j++)
                    {
                        if (cells[j] == i)
                        {
                            cells[j] = blocks.Count - 1;
                        }
                    }
                }
            }

            // Print the final state of the grid
            Debug.Log("Final state of the grid:");
            for (int row = 0; row < rows; row++)
            {
                string rowString = "";
                for (int column = 0; column < columns; column++)
                {
                    int cellIndex = column + row * columns;
                    rowString += $"{cells[cellIndex]} ";
                }
                Debug.Log(rowString);
            }
            Debug.Log($"Blocks count: {blocks.Count}");
        }


        private void UnionChunksAt(int row, int column)
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
                            Debug.Log($"Unioning chunks at ({row}, {column}) with neighbour at ({neighbourRow}, {neighbourColumn}) in Left direction");
                            bool unioned = false;
                            foreach (Chunk chunk in block.GetLeft())
                            {
                                foreach (Chunk neighbourChunk in neighbourBlock.GetRight())
                                {
                                    if (chunk.ColorCode == neighbourChunk.ColorCode)
                                    {
                                        chunkSet.Union(chunk, neighbourChunk);
                                        unioned = true;
                                    }
                                }
                            }
                            if (!unioned)
                            {
                                Debug.Log($"No matching chunks found for union at ({row}, {column}) with neighbour at ({neighbourRow}, {neighbourColumn}) in Left direction");
                                Chunk[] myLeft = block.GetLeft();
                                Chunk[] neighbourRight = neighbourBlock.GetRight();
                                string myLeftString = string.Join(", ", Array.ConvertAll(myLeft, c => c.ColorCode.ToString()));
                                Debug.Log($"My Left Chunks: {myLeftString}");
                                string neighbourRightString = string.Join(", ", Array.ConvertAll(neighbourRight, c => c.ColorCode.ToString()));
                                Debug.Log($"Neighbour Right Chunks: {neighbourRightString}");
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
                            foreach (Chunk chunk in block.GetBottom())
                            {
                                    foreach (Chunk neighbourChunk in neighbourBlock.GetTop())
                                    {
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

        private void UnionAllChunks()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    UnionChunksAt(row, column);
                }
            }

            // Print sets
            Chunk[][] chunkSetArr = chunkSet.GetAllSets();
            foreach (Chunk[] chunkSet in chunkSetArr)
            {
                if (chunkSet.Length == 0)
                {
                    continue;
                }
                string setString = "1. Set: ";
                foreach (Chunk chunk in chunkSet)
                {
                    setString += $"{chunk.ColorCode} ";
                }
                Debug.Log(setString);
            }
        }

        private Block[] RemoveNonDistinctChunks()
        {
            HashSet<Block> involvedBlocks = new HashSet<Block>();
            Chunk[] removedChunks = chunkSet.GetAndRemoveNonDistinctElements();
            foreach (Chunk removedChunk in removedChunks)
            {
                involvedBlocks.Add(removedChunk.ParentBlock);
                removedChunk.RemoveFromBlock();
            }
            if (involvedBlocks.Count == 0)
            {
                return new Block[0];
            }

            Block[] blocksToReturn = new Block[involvedBlocks.Count];
            involvedBlocks.CopyTo(blocksToReturn);

            // Print sets
            Chunk[][] chunkSetArr = chunkSet.GetAllSets();
            foreach (Chunk[] chunkSet in chunkSetArr)
            {
                if (chunkSet.Length == 0)
                {
                    continue;
                }
                string setString = "2. Set: ";
                foreach (Chunk chunk in chunkSet)
                {
                    setString += $"{chunk.ColorCode} ";
                }
                Debug.Log(setString);
            }

            return blocksToReturn;
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
        public delegate void BlockVariantChanged(Block block, BlockVariant newVariant);
        public delegate void BlockPreparedRemoval(Block block);
        public event BlockVariantChanged VariantChanged;
        public event BlockPreparedRemoval PreparedRemoval;

        private BlockVariant variant;
        private List<Chunk> chunks;


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
                
                variant = BlockVariant.DoubleVertical;
                return;
            }

            // Three colored block. 4 Variants:
            // 1. TripleLeft
            // Pos:    Color:
            // 01   -> 01
            // 23      02
            if (c0 == c2 && c1 != c3 && c0 != c1 && c0 != c3)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c1));
                chunks.Add(new Chunk(this, (ColorCode) c3));

                variant = BlockVariant.TripleLeft;
                return;
            }
            // 2.TripleTop
            // Pos:    Color:
            // 01   -> 00
            // 23      12
            if (c0 == c1 && c2 != c3 && c0 != c2 && c0 != c3)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c2));
                chunks.Add(new Chunk(this, (ColorCode) c3));

                variant = BlockVariant.TripleTop;
                return;
            }
            // 3.TripleRight
            // Pos:    Color:
            // 01   -> 01
            // 23      21
            if (c1 == c3 && c0 != c2 && c0 != c1 && c1 != c2)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c1));
                chunks.Add(new Chunk(this, (ColorCode) c2));
                
                variant = BlockVariant.TripleRight;
                return;
            }
            // 4.TripleBottom
            // Pos:    Color:
            // 01   -> 01
            // 23      22
            if (c2 == c3 && c0 != c1 && c0 != c2 && c1 != c2)
            {
                chunks.Add(new Chunk(this, (ColorCode) c0));
                chunks.Add(new Chunk(this, (ColorCode) c1));
                chunks.Add(new Chunk(this, (ColorCode) c2));

                variant = BlockVariant.TripleBottom;
                return;
            }

            // Support more if needed
        }


        public void RemoveChunk(Chunk chunk)
        {
            int removedChunkIndex = chunks.IndexOf(chunk);
            if (removedChunkIndex < 0 || removedChunkIndex >= chunks.Count)
            {
                // Chunk not found
                return;
            }
            chunks.RemoveAt(removedChunkIndex);
            // Set new variant based on the remaining chunks
            variant = Expand(variant, removedChunkIndex);
            
            VariantChanged?.Invoke(this, variant);
        }

        public Chunk[] GetLeft()
        {
            // New direction: There is always a rule for getting all the directions based on the variant.
            switch (variant)
            {
                // 00
                // 00
                case BlockVariant.Single:
                    return new Chunk[] { chunks[0] };
                // 00
                // 11
                case BlockVariant.DoubleHorizontal:
                    return new Chunk[] { chunks[0], chunks[1] };
                // 01
                // 01
                case BlockVariant.DoubleVertical:
                    return new Chunk[]{ chunks[0]};
                // 01
                // 02
                case BlockVariant.TripleLeft:
                    return new Chunk[]{ chunks[0] };
                // 00
                // 12
                case BlockVariant.TripleTop:
                    return new Chunk[]{ chunks[0], chunks[1] };
                // 01
                // 21
                case BlockVariant.TripleRight:
                    return new Chunk[]{ chunks[0], chunks[2] };
                // 01
                // 22
                case BlockVariant.TripleBottom:
                    return new Chunk[] { chunks[0], chunks[2] };
                default:
                    return new Chunk[0];
            }
        }

        public Chunk[] GetRight()
        {
            switch (variant)
            {
                // 00
                // 00
                case BlockVariant.Single:
                    return new Chunk[] { chunks[0] };
                // 00
                // 11
                case BlockVariant.DoubleHorizontal:
                    return new Chunk[] { chunks[0], chunks[1] };
                // 01
                // 01
                case BlockVariant.DoubleVertical:
                    return new Chunk[] { chunks[1] };
                // 01
                // 02
                case BlockVariant.TripleLeft:
                    return new Chunk[] { chunks[1], chunks[2] };
                // 00
                // 12
                case BlockVariant.TripleTop:
                    return new Chunk[] { chunks[0], chunks[2] };
                // 01
                // 21
                case BlockVariant.TripleRight:
                    return new Chunk[] { chunks[1] };
                // 01
                // 22
                case BlockVariant.TripleBottom:
                    return new Chunk[] { chunks[1], chunks[2] };
                default:
                    return new Chunk[0];
            }
        }

        public Chunk[] GetTop()
        {
            switch (variant)
            {
                // 00
                // 00
                case BlockVariant.Single:
                    return new Chunk[] { chunks[0] };
                // 00
                // 11
                case BlockVariant.DoubleHorizontal:
                    return new Chunk[] { chunks[0] };
                // 01
                // 01
                case BlockVariant.DoubleVertical:
                    return new Chunk[] { chunks[0], chunks[1] };
                // 01
                // 02
                case BlockVariant.TripleLeft:
                    return new Chunk[] { chunks[0], chunks[1] };
                // 00
                // 12
                case BlockVariant.TripleTop:
                    return new Chunk[] { chunks[0] };
                // 01
                // 21
                case BlockVariant.TripleRight:
                    return new Chunk[] { chunks[0], chunks[1] };
                // 01
                // 22
                case BlockVariant.TripleBottom:
                    return new Chunk[] { chunks[0], chunks[1] };
                default:
                    return new Chunk[0];
            }
        }

        public Chunk[] GetBottom()
        {
            switch (variant)
            {
                // 00
                // 00
                case BlockVariant.Single:
                    return new Chunk[] { chunks[0] };
                // 00
                // 11
                case BlockVariant.DoubleHorizontal:
                    return new Chunk[] { chunks[1] };
                // 01
                // 01
                case BlockVariant.DoubleVertical:
                    return new Chunk[] { chunks[0], chunks[1] };
                // 01
                // 02
                case BlockVariant.TripleLeft:
                    return new Chunk[] { chunks[0], chunks[2] };
                // 00
                // 12
                case BlockVariant.TripleTop:
                    return new Chunk[] { chunks[1], chunks[2] };
                // 01
                // 21
                case BlockVariant.TripleRight:
                    return new Chunk[] { chunks[0], chunks[2] };
                // 01
                // 22
                case BlockVariant.TripleBottom:
                    return new Chunk[] { chunks[2] };
                default:
                    return new Chunk[0];
            }
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

        public void PrepareForRemoval()
        {
            PreparedRemoval?.Invoke(this);
        }

        // Expand the remain chunks of the block when exactly ONE chunk is removed
        private BlockVariant Expand(BlockVariant variantBefore, int removedChunkIndex)
        {
            switch (variantBefore)
            {
                // In case of Double, the remaining chunk takes over the block.
                // The block becomes Single (so sad).
                case BlockVariant.DoubleHorizontal:
                case BlockVariant.DoubleVertical:
                    {
                        return BlockVariant.Single;
                    }
                case BlockVariant.TripleLeft:
                    {
                        // If it is a TripleLeft, then its chunk order is like this:
                        // 01
                        // 02
                        // (See Block's constructor for more info)

                        if (removedChunkIndex == 0)
                        {
                            // Turns into:
                            // 11
                            // 22
                            return BlockVariant.DoubleHorizontal;
                        } else
                        {
                            // Turns into:
                            // 02 or 01
                            // 02    01
                            return BlockVariant.DoubleVertical;
                        }

                    }
                case BlockVariant.TripleTop:
                    {
                        // If it is a TripleTop, then its chunk order is like this:
                        // 00
                        // 12
                        // (See Block's constructor for more info)

                        if (removedChunkIndex == 0)
                        {
                            // Turns into:
                            // 12
                            // 12
                            return BlockVariant.DoubleVertical;
                        } else
                        {
                            // Turns into:
                            // 00 or 00
                            // 22    11
                            return BlockVariant.DoubleHorizontal;
                        }
                    }
                case BlockVariant.TripleRight:
                    {
                        // If it is a TripleRight, then its chunk order is like this:
                        // 01
                        // 21
                        // (See Block's constructor for more info)
                        
                        if (removedChunkIndex == 0)
                        {
                            // Turns into:
                            // 21
                            // 21
                            return BlockVariant.DoubleVertical;
                        } else if (removedChunkIndex == 1)
                        {
                            // Turns into:
                            // 00
                            // 22
                            return BlockVariant.DoubleHorizontal;
                        } else
                        {
                            // Turns into:
                            // 01
                            // 01
                            return BlockVariant.DoubleVertical;
                        }
                    }
                case BlockVariant.TripleBottom:
                    {
                        // If it is a TripleTop, then its chunk order is like this:
                        // 01
                        // 22
                        // (See Block's constructor for more info)

                        if (removedChunkIndex == 0)
                        {
                            // Turns into:
                            // 11
                            // 22
                            return BlockVariant.DoubleHorizontal;
                        }
                        else if (removedChunkIndex == 1)
                        {
                            // Turns into:
                            // 00
                            // 22
                            return BlockVariant.DoubleHorizontal;
                        }
                        else
                        {
                            // Turns into:
                            // 01
                            // 01
                            return BlockVariant.DoubleVertical;
                        }
                    }
                default:
                    return BlockVariant.None;
            }
        }
    }

    // A colored chunk of a jelly block
    public class Chunk : IComparable<Chunk>
    {
        private Block parentBlock;
        private ColorCode colorCode;
        

        public Block ParentBlock { get { return parentBlock; } }


        public ColorCode ColorCode { get { return colorCode; } }


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