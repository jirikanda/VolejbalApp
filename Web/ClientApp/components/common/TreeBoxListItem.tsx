import * as React from "react";
import { getLeaves } from "../../helpers/hierarchy";
import * as classNames from "classNames";
import { TreeBoxItem } from "./TreeBox";

interface TreeBoxListItemProps {
    item: TreeBoxItem;
    itemsPath: TreeBoxItem[];
    onCheckItem: (item: TreeBoxItem) => void;
    onUncheckItem: (item: TreeBoxItem) => void;
    onCheckItems: (items: TreeBoxItem[]) => void;
    onUncheckItems: (items: TreeBoxItem[]) => void;
    onSelectItem: (item: TreeBoxItem) => void;
    onAddIndexOffsetTop: (offsetTop: number) => void;
}

const TreeBoxListItem = (props: TreeBoxListItemProps) => {
    const onClick = (event: React.MouseEvent<HTMLLIElement | HTMLInputElement>, item: TreeBoxItem) => {
        if (!(event.currentTarget && event.currentTarget instanceof HTMLInputElement)) {
            props.onSelectItem(item);
            if (item.children.length === 0) {
                props.onSelectItem(item);
                item.checked
                    ? props.onUncheckItem(item)
                    : props.onCheckItem(item);
            }
        }

        // clicked element does not have to be HTML list item - we want offsetTop of list item
        let element = event.currentTarget as HTMLElement;
        while (element && !(element instanceof HTMLLIElement)) {
            element = element.parentElement;
        }

        props.onAddIndexOffsetTop(element.offsetTop);
        event.stopPropagation();
    };

    const onChange = (event: React.ChangeEvent<HTMLInputElement>, item: TreeBoxItem) => {
        props.onSelectItem(item);
        if (item.children.length > 0) {
            const leaves = getLeaves(item);
            item.checked
                ? props.onUncheckItems(leaves)
                : props.onCheckItems(leaves);
        } else {
            item.checked
                ? props.onUncheckItem(item)
                : props.onCheckItem(item);
        }
    };

    const setCheckBoxState = (checkbox: HTMLInputElement, item: TreeBoxItem) => {
        if (checkbox) {
            checkbox.checked = item.checked;
            checkbox.indeterminate = item.indeterminate;
        }
    }

    const { item, itemsPath } = props;

    const isLeaf = (item.children.length > 0);
    const isActive = itemsPath.some(itemPath => itemPath.id === item.id);

    return <li className={classNames("treebox-list-item p-2 pr-4", { "leaf": isLeaf, "active": isActive })} onClick={event => onClick(event, item)}>
        <span>
            <input
                id={`treebox-checkbox-${item.id}`}
                type="checkbox"
                onChange={event => onChange(event, item)}
                onClick={event => onClick(event, item)}
                ref={checkbox => setCheckBoxState(checkbox, item)} />
            {" "}
            <label className="treebox-list-item-label">
                {`${item.name}`}
                {(item.children.length > 0 && item.checkedDescendantsCount > 0)
                    ? ` (${item.checkedDescendantsCount} / ${item.descendantsCount})`
                    : ""}
            </label>
        </span>
    </li>
}

export default TreeBoxListItem;