function [ decision ] = albumDecideSVM( userWorkSpace, kernel )

%decide

fileToDecide = strcat(userWorkSpace,'\\Decide.txt');
profilePath = strcat(userWorkSpace,'\\profile');
profilePath = strcat(profilePath, kernel);
profilePath = strcat(profilePath, '.mat');
resultFilePath = strcat(userWorkSpace,'\\Result.txt');

load(profilePath,'svmStruct');

M = csvread(fileToDecide);
M = M(1:1, 1:15);

decision = svmclassify(svmStruct,M);
resultFile = fopen(resultFilePath,'w');
fprintf(resultFile,'%d',decision);
fclose(resultFile);
%hold on;plot(5,2,'ro','MarkerSize',12);hold off

end

