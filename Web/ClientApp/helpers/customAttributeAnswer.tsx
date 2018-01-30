import { CustomAttributeValue } from '../apimodels/ProductMatrixStore';
import * as React from 'react';

const getCustomAttributeAnswer = (customAttributeAnswer: CustomAttributeValue): string | JSX.Element => {
    let answerContent: string | JSX.Element;

    if (customAttributeAnswer.valueIsMissing) {
        answerContent = "Chybí";
    } else if (customAttributeAnswer.valueIsNotSet) {
        answerContent = "Nestanoveno"
    } else if (customAttributeAnswer.valueIsIrrelevant) {
        answerContent = <span className="irrelevant">&times;</span>;
    } else {
        answerContent = customAttributeAnswer.text;
    }

    return answerContent;
}

export default getCustomAttributeAnswer;