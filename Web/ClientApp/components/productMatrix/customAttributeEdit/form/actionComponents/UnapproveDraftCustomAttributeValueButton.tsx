import * as React from 'react';
import { Button } from 'reactstrap';

interface UnapproveDraftCustomAttributeValueButtonProps {
    canUserSubmit: boolean;
    onClick: (event: React.MouseEvent<HTMLButtonElement>) => void;
}

const UnapproveDraftCustomAttributeValueButton = (props: UnapproveDraftCustomAttributeValueButtonProps) =>
    <Button color="primary" type="submit" disabled={!props.canUserSubmit} onClick={props.onClick}>Odemknout pro editaci (Odvolat schválení)</Button>

export default UnapproveDraftCustomAttributeValueButton;