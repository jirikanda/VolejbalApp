import * as React from 'react';
import { Dropdown, DropdownToggle, DropdownMenu, DropdownItem, Input, Label } from 'reactstrap';

interface CheckboxDropDownListItem {
    id: number;
    name: string;
}

type CheckboxDropDownListProps = {
    title: string;
    items: CheckboxDropDownListItem[];
    selectedItems: CheckboxDropDownListItem[];
    onItemSelect: (item: CheckboxDropDownListItem) => void;
    onItemUnselect: (item: CheckboxDropDownListItem) => void;
    onItemsSelect: (item: CheckboxDropDownListItem[]) => void;
    onItemsUnselect: (item: CheckboxDropDownListItem[]) => void;
}

interface CheckboxDropDownListState {
    isDropDownOpen: boolean;
}

{ /* HACKED COMPONENT: definition types for Reactstrap are incorrect: see https://github.com/DefinitelyTyped/DefinitelyTyped/pull/20867
    - cannot use Label and Input Reactstrap components
    - after pull request sync please update definition typed package for Reactstrap and fix this component with Labe and Input
    - function dropdownToggle should not be so logic - it should be simple as much as possible
*/ }

class CheckboxDropDownList extends React.Component<CheckboxDropDownListProps, CheckboxDropDownListState> {
    wasForcedToClick: boolean;

    constructor(props) {
        super(props);

        this.state = {
            isDropDownOpen: false
        }
    }

    onChangeOne = (checked: boolean, item: CheckboxDropDownListItem) => checked ? this.props.onItemSelect(item) : this.props.onItemUnselect(item);
    onChangeAll = (checked: boolean, items: CheckboxDropDownListItem[]) => checked ? this.props.onItemsSelect(items) : this.props.onItemsUnselect(items);

    dropdownToggle = () => {
        if (document.activeElement.parentElement.id.startsWith("wrapper-checkboxdropdown-item") || document.activeElement.id.startsWith("wrapper-checkboxdropdown-item")) {
            this.setState(prevState => ({ isDropDownOpen: true }));

            if (!(document.activeElement instanceof HTMLInputElement) && !this.wasForcedToClick) {
                this.wasForcedToClick = true;
                const checkbox = document.activeElement.children[0] as HTMLInputElement;
                checkbox.click();
            }
            else {
                this.wasForcedToClick = false;
            }
            if ((document.activeElement instanceof HTMLLabelElement) && !this.wasForcedToClick) {
                this.wasForcedToClick = true;
                const checkbox = document.activeElement.parentElement.children[0] as HTMLInputElement;
                checkbox.click();
            }
            else {
                this.wasForcedToClick = false;
            }
        } else {
            if (!(document.activeElement instanceof HTMLInputElement)) {
                this.setState(prevState => ({ isDropDownOpen: !prevState.isDropDownOpen }));
            }
        }

    }

    setCheckBoxState = (checkbox: HTMLInputElement, selectedCount: number, itemsCount: number) => {
        if (checkbox) {
            checkbox.indeterminate = (selectedCount > 0) && (itemsCount > selectedCount);
            checkbox.checked = (selectedCount === itemsCount);
        }
    }

    render() {
        const {
            title,
            items,
            selectedItems,
            onItemSelect,
            onItemUnselect,
            onItemsSelect,
            onItemsUnselect
        } = this.props;

        const selectedCount = selectedItems
            .filter(selectedItem => items.some(item => (selectedItem.id === item.id)))
            .length;

        const itemsCount = items.length;

        const itemCheckBoxes = [];

        return <Dropdown toggle={this.dropdownToggle}  isOpen={this.state.isDropDownOpen}>
            <DropdownToggle caret>
                <input
                    type="checkbox"
                    ref={element => this.setCheckBoxState(element, selectedCount, itemsCount)}
                    onChange={event => this.onChangeAll(event.target.checked, items)}
                    onClick={event => event.stopPropagation()} />
                {`${title} (${selectedCount}/${itemsCount})`}
            </DropdownToggle>
            <DropdownMenu>
                {items.map(item =>
                    <span key={`checkboxdropdown-item-${item.id}`} id={`wrapper-checkboxdropdown-item-${item.id}`}>
                        <DropdownItem>
                            <input
                                type="checkbox"
                                name={`checkboxdropdown-item-${item.id}`}
                                id={`checkboxdropdown-item-${item.id}`}
                                value={item.name}
                                checked={selectedItems.find(selectedItem => selectedItem.id === item.id) !== undefined}
                                onChange={event => this.onChangeOne(event.target.checked, item)} />
                            <label>
                                {item.name}
                            </label>
                    </DropdownItem></span>)}
            </DropdownMenu>
        </Dropdown>
    }
}

export default CheckboxDropDownList;