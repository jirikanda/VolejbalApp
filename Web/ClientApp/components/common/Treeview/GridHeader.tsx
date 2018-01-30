/* tslint:disable */

import * as React from 'react';
import { ColumnConfig } from './GridInterfaces';

interface GridHeaderProps {
    columns: ColumnConfig[]
}

const GridHeader = (props: GridHeaderProps) => {
    const {columns} = props;
    const headerCells = columns.map((column, index) => 
        <div key={index} className="grid-cell">
            {column.displayName}
        </div>
    );
    
    return <div className="grid-header grid-row">
        {headerCells}
    </div> 
}

export default GridHeader;