import * as React from 'react';
import { Button} from 'reactstrap';

interface UpdateCurrentCustomAttributeValueButtonProps {
    canUserSubmit: boolean;
    onClick: (event: React.MouseEvent<HTMLButtonElement>) => void;
}

const UpdateCurrentCustomAttributeValueButton = (props: UpdateCurrentCustomAttributeValueButtonProps) =>
    <Button color="primary" type="submit" disabled={!props.canUserSubmit} onClick={props.onClick}>Opravit publikované aktuální hodnoty</Button>

export default UpdateCurrentCustomAttributeValueButton;