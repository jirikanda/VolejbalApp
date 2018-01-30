import * as React from 'react';
import { Button } from 'reactstrap';

interface ApproveDraftCustomAttributeValueButtonProps {
    canUserSubmit: boolean;
    onClick: (event: React.MouseEvent<HTMLButtonElement>) => void;
}

const ApproveDraftCustomAttributeValueButton = (props: ApproveDraftCustomAttributeValueButtonProps) =>
    <Button color="primary" type="submit" disabled={!props.canUserSubmit} onClick={props.onClick}>Uložit, Schválit a Publikovat novou verzi</Button>

export default ApproveDraftCustomAttributeValueButton;