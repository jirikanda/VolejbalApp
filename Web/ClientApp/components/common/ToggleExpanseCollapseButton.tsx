import * as React from 'react';
import { Button } from 'reactstrap';

interface ToggleExpanseCollapseButtonProps {
    isTextCollapsed: boolean;
    isVisible: boolean;
    toggleExpanseCollapseButtonClicked: () => void;
}

const ToggleExpanseCollapseButton = (props: ToggleExpanseCollapseButtonProps) => {
    const { isTextCollapsed, isVisible, toggleExpanseCollapseButtonClicked } = props;

    return isVisible && <Button color="link" className="p-0 product-matrix-item-toggle" onClick={toggleExpanseCollapseButtonClicked}>
        {isTextCollapsed ? <span>Více <i className="fa fa-angle-right" aria-hidden="true"></i></span> : <span><i className="fa fa-angle-left" aria-hidden="true"></i> Méně</span>}
    </Button>
}

export default ToggleExpanseCollapseButton;