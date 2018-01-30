import * as React from 'react';
import { Button } from 'reactstrap';

interface ToggleAllProps {
    isAnySelected: boolean;
    onSelectAll: () => void;
    onUnselectAll: () => void;
}

const ToggleAll = (props: ToggleAllProps) =>
    <Button color="link" onClick={event => props.isAnySelected ? props.onUnselectAll() : props.onSelectAll()}>
        {props.isAnySelected ? "Zrušit výběr" : "Označit vše"}
    </Button>

export default ToggleAll