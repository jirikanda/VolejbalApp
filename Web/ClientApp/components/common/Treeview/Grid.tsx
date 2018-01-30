import * as React from 'react';

import GridHeader from './GridHeader';
import { GridConfig, GridData, GridRow as GridRowInterface } from './GridInterfaces';
import GridRow from './GridRow';

interface GridProps {
    gridConfig: GridConfig;
    data: GridData;
}

const getRowsComponentWithIndent = (row: GridRowInterface, level: number, gridConfig: GridConfig): JSX.Element[] => {
    const result = row.children.map((child, index) => {

        let temp = [<GridRow {...gridConfig} gridRow={child} key={`${row.rowType}-${row.id}-${index}-${level + 1}`} level={level + 1} />];
        if (child.children.length > 0) {
            temp = temp.concat(getRowsComponentWithIndent(child, level + 1, gridConfig));
        }
        return temp;
    });

    return result.reduce((a, b) => a.concat(b));
}

const Grid = (props: GridProps) => {
    const {gridConfig, data} = props;

    const dataRows = data.rows.map((row, index) => {
        const rows = [<GridRow {...gridConfig} gridRow={row} key={`${row.rowType}-${row.id}-${index}-0`} level={0} />];

        if (row.children.length > 0) {
            return rows.concat(getRowsComponentWithIndent(row, 0, gridConfig));
        }
        return rows
    });

    return <div className="treeview-grid">
        <GridHeader columns={gridConfig.columns} />
        {dataRows.reduce((a, b) => a.concat(b))}
    </div>
}

export default Grid;