import * as React from 'react';
import { Label } from 'reactstrap';

interface CurrentCopyToDraftButtonProps {
    openModal: (event: React.MouseEvent<HTMLLabelElement>) => void;
}

const CurrentCopyToDraftButton = (props: CurrentCopyToDraftButtonProps) =>
        <i className="fa fa-arrow-right pointer" aria-hidden="true" onClick={props.openModal} title="Kopírovat aktuální hodnoty do konceptu"></i>

export default CurrentCopyToDraftButton