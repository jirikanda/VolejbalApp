import * as React from "react";
import { getLeaves, INode } from "../../helpers/hierarchy";
import TreeBoxList from "./TreeBoxList";

export interface TreeBoxItem extends INode {
  id: number;
  children: TreeBoxItem[];
  name?: string;
  checked?: boolean;
  indeterminate?: boolean;
  visible?: boolean;
  checkedDescendantsCount?: number;
  descendantsCount?: number;
}

interface TreeBoxProps {
  items: TreeBoxItem[];
  checkedItems: TreeBoxItem[];
  visibleItems: TreeBoxItem[];
  itemsPath: TreeBoxItem[];
  onCheckItem: (item: TreeBoxItem) => void;
  onUncheckItem: (item: TreeBoxItem) => void;
  onCheckItems: (items: TreeBoxItem[]) => void;
  onUncheckItems: (items: TreeBoxItem[]) => void;
  onSelectItem: (item: TreeBoxItem) => void;
}

interface TreeBoxListIndexOffsetTop {
    index: number;
    offsetTop: number;
}

interface TreeBoxState {
    indexOffsetTops: TreeBoxListIndexOffsetTop[];
}

export default class TreeBox extends React.Component<TreeBoxProps, TreeBoxState> {
    constructor(props: TreeBoxProps) {
        super(props);

        this.state = {
            indexOffsetTops: []
        }
    }

    getTreeBoxItemsAsMatrix = (items: TreeBoxItem[], checkedItems: TreeBoxItem[], visibleItems: TreeBoxItem[]) => {
        const verticalDimensionTreeBoxItems: TreeBoxItem[][] = [];
        let treeboxItems = items;

        while (treeboxItems.length > 0) {
            verticalDimensionTreeBoxItems.push(treeboxItems);
            treeboxItems = treeboxItems.map(item => item.children).reduce((a, b) => a.concat(b));
        }

        // traversing bottom-up to set check/indeterminate state -> reverse
        verticalDimensionTreeBoxItems.reverse().forEach(treeBoxItems => {
            treeBoxItems.forEach(item => {
                if (item.children.length === 0) {
                    item.checked = checkedItems.find(checkedItem => checkedItem.id === item.id) !== undefined;
                } else {
                    const itemLeaves = getLeaves(item) as TreeBoxItem[];
                    item.checkedDescendantsCount = itemLeaves.filter(childItem => childItem.checked).length;
                    item.descendantsCount = itemLeaves.length;
                    item.checked = item.checkedDescendantsCount === item.descendantsCount;
                    item.indeterminate = item.children.some(childItem => childItem.checked || childItem.indeterminate) && !item.checked;
                }
                item.visible = visibleItems.some(visibleItem => visibleItem.id === item.id);
            });
        });

        if (verticalDimensionTreeBoxItems.length > 0) {
            verticalDimensionTreeBoxItems.reverse();
            verticalDimensionTreeBoxItems[0].forEach(item => (item.visible = true));
        }

        return verticalDimensionTreeBoxItems;
    };

    calculateOffsetTop = (visibleItemsIndex: number) => {
        const indexOffsetTop = this.state.indexOffsetTops.find(item => item.index === visibleItemsIndex);
        let offsetTop = 0;
        if (indexOffsetTop) {
            offsetTop = indexOffsetTop.offsetTop;

            const previousOffsetTops = this.state.indexOffsetTops.filter(item => item.index < visibleItemsIndex); // take previous
            if (previousOffsetTops && previousOffsetTops.length > 0) {
                offsetTop += previousOffsetTops.map(previousItem => previousItem.offsetTop).reduce((a, b) => a + b, 0);
            }
        }
        return offsetTop;
    }

    addIndexOffsetTop = (index: number, offsetTop: number) => {
        const indexOffsetTop = this.state.indexOffsetTops.find(item => item.index === index);
        const indexOffsetTopIndex = this.state.indexOffsetTops.findIndex(item => item.index === index);

        if (indexOffsetTop && indexOffsetTopIndex > -1) { 

            const newIndexOffsetTops = this.state.indexOffsetTops.filter(item => item.index !== index);

            this.setState((prevState) => ({
                indexOffsetTops: newIndexOffsetTops.concat({ index, offsetTop })
            }));
        } else {
            const newIndexOffsetTops = this.state.indexOffsetTops.concat({ index, offsetTop });
            this.setState((prevState) => ({ indexOffsetTops: newIndexOffsetTops }));
        }
    }

    render() {
        const { items, checkedItems, visibleItems, itemsPath, onCheckItem, onCheckItems, onSelectItem, onUncheckItem, onUncheckItems } = this.props;
        const treeBoxItemsMatrix = this.getTreeBoxItemsAsMatrix(items, checkedItems, visibleItems);

        const listsToRender = treeBoxItemsMatrix
            .filter(items => items.some(item => item.visible))
            .map((visibleItems, visibleItemsIndex, visibleTreeBoxItemsMatrix) => {
                const offsetTop = this.calculateOffsetTop(visibleItemsIndex);
                return <TreeBoxList
                    key={`treebox-list-${visibleItemsIndex}`}
                    listItems={visibleItems}
                    topMargin={offsetTop}
                    onAddIndexOffsetTop={offsetTop => this.addIndexOffsetTop(visibleItemsIndex + 1, offsetTop)}
                    {...this.props} />;
            });

        return <div className="treebox mb-3">{listsToRender}</div>;
    }
}