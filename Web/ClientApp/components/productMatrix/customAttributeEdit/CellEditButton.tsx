import * as React from 'react';

interface CellEditButtonProps {
    editButtonClicked: () => void;
}

const CellEditButton = (props: CellEditButtonProps) => (
    <span className="product-matrix-item-edit-button float-right mr-2" onClick={props.editButtonClicked}>
        <i className="fa fa-pencil" aria-hidden="true"></i>
    </span>)

export default CellEditButton;