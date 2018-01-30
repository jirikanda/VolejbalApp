import * as React from 'react';
import { findDOMNode } from 'react-dom'
import { DragSource, DragSourceSpec, DragSourceConnector,
    DragSourceMonitor, ConnectDragSource, DropTargetConnector, DropTarget,
    DropTargetMonitor, ConnectDropTarget, DropTargetSpec } from 'react-dnd';
import { ProductVariantDocumentsRowEditableProps } from './ProductVariantDocumentsRowEditable';

export enum ItemTypes {
    ProductVariantDocument = "METHODOLOGY_DOCUMENT"
}

export interface ItemProps {
    index: number;
    id: number;
}

export interface ItemFunctionProps {
    currentItemId?: number;
    isDragging?: boolean;
    connectDragSource?: ConnectDragSource;
    connectDropTarget?: ConnectDropTarget;
    // beginMoveDocument: (currentId: number, currentIndex: number) => ItemProps;
}

export type ItemAndDocumentComponentProps = ItemFunctionProps & ProductVariantDocumentsRowEditableProps & ItemProps;

export const itemTargetSpec: DropTargetSpec<ItemAndDocumentComponentProps> = {
	hover(props: ItemAndDocumentComponentProps, monitor: DropTargetMonitor, component: React.Component<ItemAndDocumentComponentProps>) {
        const {changeProductVariantDocumentsOrder, currentItemId} = props;
        const dragItem = (monitor.getItem() as ItemProps);
		const dragIndex = dragItem.index;
		const hoverIndex = props.index;

		// Don't replace items with themselves
		if (dragIndex === hoverIndex) {
			return;
		}

		// Determine rectangle on screen
		const hoverBoundingRect = findDOMNode(component).getBoundingClientRect();

		// Get vertical middle
		const hoverMiddleY = (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2;

		// Determine mouse position
		const clientOffset = monitor.getClientOffset();

		// Get pixels to the top
		const hoverClientY = clientOffset.y - hoverBoundingRect.top;

		// Only perform the move when the mouse has crossed half of the items height
		// When dragging downwards, only move when the cursor is below 50%
		// When dragging upwards, only move when the cursor is above 50%

		// Dragging downwards
		if (hoverIndex && (dragIndex < hoverIndex && hoverClientY < hoverMiddleY)) {
			return;
		}

		// Dragging upwards
		if (hoverIndex && (dragIndex > hoverIndex && hoverClientY > hoverMiddleY)) {
			return;
		}

        // Time to actually perform the action
        if(currentItemId) {
            changeProductVariantDocumentsOrder(hoverIndex, currentItemId);
            dragItem.index = hoverIndex;
		}
	}
}

export const itemSourceSpec: DragSourceSpec<ItemAndDocumentComponentProps> = {
  	beginDrag: (props: ItemAndDocumentComponentProps): ItemProps => {
		const {id, index} = props;

		return {id, index};
	}
};

export const sourceCollector = (connect: DragSourceConnector, monitor: DragSourceMonitor) => ({
    connectDragSource: connect.dragSource(),
    isDragging: monitor.isDragging(),
	currentItemId: monitor.getItem() && (monitor.getItem() as ItemProps).id
});

export const targetCollector = (connect: DropTargetConnector) => ({
	connectDropTarget: connect.dropTarget()
});
