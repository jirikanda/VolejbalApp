import * as React from 'react';
import * as classNames from "classNames";

import { GridConfig, GridRow, GridRowType } from './GridInterfaces';
import GridCell from './GridCell';

interface GridRowProps {
    gridRow: GridRow;
    level: number;
    onRowClick?: (rowId: number, rowType: GridRowType) => void;
}

const GridRow = ({ gridRow, level, onRowClick }: GridRowProps) => {
    const indent = [];
    for (let i = 0; i < level; i++) {
        indent.push(<div className="indent" key={i}></div>)
    }

    const cells = gridRow.cells.map((cell, index) =>
        <GridCell gridCell={cell} key={index} />
    );

    const handleOnRowClick = () => onRowClick(gridRow.id, gridRow.rowType);

    const classes = classNames("grid-row", { "grid-row-selected": gridRow.isSelected })

    return <div onClick={handleOnRowClick} className={classes}>
        {indent}
        {cells}
    </div>
}

export default GridRow;