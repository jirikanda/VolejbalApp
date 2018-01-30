export enum GridRowType {
    Topic = 1,
    CustomAttribute = 2
}

export interface ColumnConfig {
    /**
     * Identification of particular column used to connect this column to the given data with the same columnName
     */
    columnName: string,

    /**
     * Column name that will be displayed in the Grid header
     */
    displayName: string
}

export interface GridConfig {
    /**
     * Custom css classes for additional styling purpose
     */
    customClasses?: {
        grid?: string;
        row?: string;
        cell?: string;
        headerRow?: string;
        headerCell?: string;
    };

    onRowClick?: (rowId: number, rowType: GridRowType) => void;

    columns: ColumnConfig[];     // kolik tu bude objektů, tolik bude sloupců v gridu a v daném pořadí + případně tree column
}

// melo by to byt INode kompatibilni -  {id: number, children: INode[]}
export interface GridCell {
    /**
     * Text that will be displayed as a cell content
     */
    name: string;   // sem přijde topic name

    /**
     * Identification of the given data used to connect to the given data with the same columnName
     */
    // columnName: string;
    /*
     * Class names applied to cell
     */
    classNames?: string[];
}

export interface GridRow {
    /**
     * Unique indentification
     */
    id: number;    // sem prijde topic id

    /**
     * Is this row selected
     */
    isSelected: boolean;

    /**
     * Cells of this row - number of given cells must match the number of defined columns
     */
    cells: GridCell[]

    /**
     * Children of this row. Empty array means no children.
     */
    children: GridRow[];

    /**
     * Type of particular row
     */
    rowType: GridRowType;    
}

export interface GridData {
    // treeColumn?: GridTreeNode[];
    rows: GridRow[];
}