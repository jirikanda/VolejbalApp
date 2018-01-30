import * as React from 'react';
import { Button } from 'reactstrap';

interface SaveDraftCustomAttributeValueButtonProps {
    canUserSubmit: boolean;
    onClick: (event: React.MouseEvent<HTMLButtonElement>) => void;
}

const SaveDraftCustomAttributeValueButton = (props: SaveDraftCustomAttributeValueButtonProps) =>
    <Button color="primary" type="submit" disabled={!props.canUserSubmit} onClick={props.onClick}>Uložit koncept nové verze</Button>

export default SaveDraftCustomAttributeValueButton;