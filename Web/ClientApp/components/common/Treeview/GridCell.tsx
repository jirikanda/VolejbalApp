import * as React from "react";
import { GridCell } from "./GridInterfaces";
import * as classNames from "classNames";

interface GridCellProps {
    gridCell: GridCell;
}

const GridCell = ({ gridCell }: GridCellProps) => {

    const classes = classNames("grid-cell", gridCell.classNames);

    return <div className={classes}>
        {gridCell.name}
    </div>
}

export default GridCell;