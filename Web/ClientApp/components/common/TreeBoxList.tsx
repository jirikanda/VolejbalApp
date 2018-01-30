import * as React from "react";
import { TreeBoxItem } from "./TreeBox";
import TreeBoxListItem from "./TreeBoxListItem";

interface TreeBoxListProps {
    listItems: TreeBoxItem[];
    itemsPath: TreeBoxItem[];
    topMargin: number;
    onCheckItem: (item: TreeBoxItem) => void;
    onUncheckItem: (item: TreeBoxItem) => void;
    onCheckItems: (items: TreeBoxItem[]) => void;
    onUncheckItems: (items: TreeBoxItem[]) => void;
    onSelectItem: (item: TreeBoxItem) => void;
    onAddIndexOffsetTop: (offsetTop: number) => void;
}

const TreeBoxList = (props: TreeBoxListProps) => 
    <ul className="treebox-list" style={{ marginTop: `${props.topMargin}px` }}>
        {props.listItems.filter(item => item.visible).map(item => <TreeBoxListItem key={item.id} item={item} {...props} />)}
    </ul>

export default TreeBoxList