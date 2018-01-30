import * as React from 'react';
import { Field, Validator } from 'redux-form';
import { Label } from 'reactstrap';
import { CustomAttributeValueVM, GetCustomAttributeValueDetailVM, CustomAttributeVM, CustomAttributeType } from '../../../../../apimodels/CustomAttributeValueStore';
import * as classNames from 'classNames';
import * as moment from 'moment';
import { BooleanExtended } from '../../../../../apimodels/CustomAttributeValueStore';
import { Input, FormFeedback } from 'reactstrap';
import { capitalize } from '../../../../../helpers/string';
import ValueStringComponent from './ValueStringComponent';
import ValueDecimalComponent from './ValueDecimalComponent';
import ValueIntComponent from './ValueIntComponent';
import ValueBooleanComponent from './ValueBooleanComponent';
import ValueBooleanExtendedComponent from './ValueBooleanExtendedComponent';
import ValueDateComponent from './ValueDateComponent';

export interface ValueComponentProps {
    id: string;
    name: string;
    customAttributeValue: CustomAttributeValueVM;
    customAttribute: CustomAttributeVM;
    onValueChange: (event, id: string) => void;
    onSimulateValueClick: (id: string) => void;
    disabled?: boolean;
    validate?: Validator | Validator[];
}

class ValueComponent extends React.Component<ValueComponentProps> {
    getValueComponent = (props: ValueComponentProps) => {
        switch (props.customAttribute.symbol) {
            case CustomAttributeType.Text:
                return <ValueStringComponent {...props} />
            case CustomAttributeType.Integer:
                return <ValueIntComponent {...props} />
            case CustomAttributeType.Decimal:
                return <ValueDecimalComponent step={0.01} {...props} />
            case CustomAttributeType.Boolean:
                return <ValueBooleanComponent {...props} />
            case CustomAttributeType.BooleanExtended:
                return <ValueBooleanExtendedComponent {...props} />
            case CustomAttributeType.Date:
                return <ValueDateComponent {...props} />
            default:
                throw new Error("Unknown custom attribute type.");
        }
    }

    render() {
        return <>
            {this.getValueComponent(this.props)}
            <label key={`Value${capitalize(this.props.name)}ComponentHiddenLabel`} style={{ display: 'none' }} htmlFor={this.props.id}></label>
        </>
    }
}

export default ValueComponent